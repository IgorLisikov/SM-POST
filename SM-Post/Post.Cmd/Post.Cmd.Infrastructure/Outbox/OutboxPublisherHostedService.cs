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
                try
                {
                    using (var scope = _serviceProvider.CreateScope())   // create scope on each iteration => eventHandler (Scoped) will be created; Repositories will be created
                    {
                        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                        var eventProducer = scope.ServiceProvider.GetRequiredService<IEventProducer>();

                        await foreach (var msg in outboxRepository.SubscribeAsync(token))
                        {
                            // if something throws here, the subscription will be restarted after 5 seconds and the event will be processed again:
                            var options = new JsonSerializerOptions { Converters = { new EventJsonConverter() } };
                            var @event = JsonSerializer.Deserialize<BaseEvent>(msg.PayloadJson, options);

                            // The order of execution makes it at least once delivery:
                            await eventProducer.ProduceAsync(_kafkaTopics.SocialMediaPostEvents, @event, token);
                            await outboxRepository.MarkAsPublishedAsync(msg.Id, DateTime.UtcNow, token);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Outbox Publisher cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Outbox Publisher failed.");
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
            }
        }
    }
}
