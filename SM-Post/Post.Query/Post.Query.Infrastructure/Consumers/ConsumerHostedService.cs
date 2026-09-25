using Confluent.Kafka;
using CQRS.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Post.Common.Converters;
using Post.Query.Infrastructure.Handlers;
using System.Text.Json;

namespace Post.Query.Infrastructure.Consumers
{
    public class ConsumerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConsumerConfig _config;

        public ConsumerHostedService(IServiceProvider serviceProvider, IOptions<ConsumerConfig> config)
        {
            _serviceProvider = serviceProvider;
            _config = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
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

                using (var scope = _serviceProvider.CreateScope())
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
    }
}
