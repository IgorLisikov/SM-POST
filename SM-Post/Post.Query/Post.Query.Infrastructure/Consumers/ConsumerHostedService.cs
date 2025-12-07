using Confluent.Kafka;
using CQRS.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Post.Query.Infrastructure.Converters;
using Post.Query.Infrastructure.Handlers;
using System.Text.Json;

namespace Post.Query.Infrastructure.Consumers
{
    // Hosted service that represents a background task.
    // Listens for new events messages from Kafka.
    // StartAsync(CancellationToken) - Called when the application starts, allowing initialization of background tasks.
    // StopAsync(CancellationToken) - Called when the application shuts down, enabling cleanup operations.
    public class ConsumerHostedService : IHostedService
    {
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConsumerConfig _config;

        private CancellationTokenSource _cts;
        private Task _backgroundTask;

        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, IServiceProvider serviceProvider, IOptions<ConsumerConfig> config)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _config = config.Value;
        }

        // The reason the scope is created on each iteartion - is due to the scoped lifetime of the IEventHandler and repositories,
        // while the ConsumerHostedService is a singleton (because it implements IHostedService).
        // IHostedService objects are typically registered as singletons by the hosting infrastructure.

        // Mismatch Between Service Lifetimes: If you try to inject a scoped service (like IEventHandler)
        // into a singleton service (like ConsumerHostedService) through the constructor, it will lead to an error.

        // By using IServiceProvider.CreateScope() in the loop, the ConsumerHostedService explicitly creates a new scope
        // on each iteartion. This allows it to resolve scoped services (like IEventHandler) within that scope. Once the scope is disposed,
        // the scoped services are also disposed, which helps maintain proper lifecycle management.

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _backgroundTask = Task.Run(() => RunConsumerLoop(_cts.Token), _cts.Token);
            return Task.CompletedTask;

            // By using Task.Run(), the work is offloaded to a separate thread,
            // without blocking the main execution thread.
            // This allows StartAsync() to return quickly.
            // If Consume() was 'async', instead of Task.Run() it would be needed to just 'await' it.
        }

        private async Task RunConsumerLoop(CancellationToken token)
        {
            var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");

            using var consumer = new ConsumerBuilder<string, string>(_config)  // Don’t register Kafka consumers in DI as singleton or scoped.
                .SetKeyDeserializer(Deserializers.Utf8)                        // Create them manually where needed, and dispose them properly.
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();

            consumer.Subscribe(topic);

            while (!token.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(token);    // Consume() blocks until consumeResult is returned
                if (consumeResult?.Message == null) continue;

                var options = new JsonSerializerOptions { Converters = { new EventJsonConverter() } };
                var @event = JsonSerializer.Deserialize<BaseEvent>(consumeResult.Message.Value, options);

                using (var scope = _serviceProvider.CreateScope())   // create scope on each iteration => eventHandler (Scoped) will be created; Repositories will be created
                {
                    var eventHandler = scope.ServiceProvider.GetRequiredService<IEventHandler>();
                    var handlerMethod = eventHandler.GetType().GetMethod("On", new[] { @event.GetType() });

                    if (handlerMethod == null)
                        throw new InvalidOperationException($"No handler found for event type {@event.GetType().Name}");

                    var task = (Task)handlerMethod.Invoke(eventHandler, new object[] { @event });
                    await task;
                }

                consumer.Commit(consumeResult);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Kafka consumer service...");
            _cts.Cancel();

            if (_backgroundTask != null)
            {
                try
                {
                    await _backgroundTask; // wait for loop to finish
                }
                catch (OperationCanceledException)
                {
                    // expected when shutting down
                }
            }
        }
    }
}
