using CQRS.Core.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // The reason the eventConsumer is created using IServiceProvider within the StartAsync method and not injected via the constructor
        // is due to the scoped lifetime of the IEventConsumer service, while the ConsumerHostedService is a singleton (because it implements IHostedService).
        // IHostedService objects are typically registered as singletons by the hosting infrastructure.

        // Mismatch Between Service Lifetimes: If you try to inject a scoped service (like IEventConsumer)
        // into a singleton service (like ConsumerHostedService) through the constructor, it will lead to an error.

        // By using IServiceProvider.CreateScope() in the StartAsync method, the ConsumerHostedService explicitly creates a new scope.
        // This allows it to resolve scoped services (like IEventConsumer) within that scope. Once the scope is disposed,
        // the scoped services are also disposed, which helps maintain proper lifecycle management.

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Event consumer service running.");

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                var eventConsumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();
                string topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
                Task.Run(() => eventConsumer.Consume(topic), cancellationToken);  // By using Task.Run(), the work is offloaded to a separate thread,
            }                                                                     // without blocking the main execution thread.
                                                                                  // This allows StartAsync() to return quickly.
                                                                                  // If Consume() was 'async', instead of Task.Run() it would be needed to just 'await' it.
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Event consumer service stopped.");

            return Task.CompletedTask;
        }
    }
}
