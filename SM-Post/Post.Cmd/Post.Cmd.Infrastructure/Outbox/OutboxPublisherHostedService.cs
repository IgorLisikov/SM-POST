using CQRS.Core.Events;
using CQRS.Core.Outbox;
using CQRS.Core.Producers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Post.Common.Configs;
using Post.Common.Converters;
using System.Text.Json;

namespace Post.Cmd.Infrastructure.Outbox
{
    public class OutboxPublisherHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxPublisherHostedService> _logger;
        private readonly KafkaTopics _kafkaTopics;

        public OutboxPublisherHostedService(
            ILogger<OutboxPublisherHostedService> logger, 
            IServiceProvider serviceProvider, 
            IOptions<KafkaTopics> kafkaTopics)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _kafkaTopics = kafkaTopics.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())   // create scope on each iteration => eventHandler (Scoped) will be created; Repositories will be created
                {
                    var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                    var eventProducer = scope.ServiceProvider.GetRequiredService<IEventProducer>();

                    var pending = await outboxRepository.GetUnpublishedAsync(100, token);
                    foreach (var msg in pending)
                    {
                        try
                        {
                            var options = new JsonSerializerOptions { Converters = { new EventJsonConverter() } };
                            var @event = JsonSerializer.Deserialize<BaseEvent>(msg.PayloadJson, options);

                            await eventProducer.ProduceAsync(_kafkaTopics.SocialMediaPostEvents, @event, token);

                            // The order of execution makes it at least once delivery.
                            await outboxRepository.MarkAsPublishedAsync(msg.Id, DateTime.UtcNow, token);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to publish outbox message {OutboxId}", msg.Id);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }
    }
}
