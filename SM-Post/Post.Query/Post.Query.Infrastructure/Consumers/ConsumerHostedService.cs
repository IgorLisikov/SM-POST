using Confluent.Kafka;
using CQRS.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Post.Common.Configs;
using Post.Common.Converters;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;
using Post.Query.Infrastructure.Handlers;
using System.Text.Json;

namespace Post.Query.Infrastructure.Consumers
{
    public class ConsumerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly KafkaConfig _kafkaConfig;
        private readonly KafkaTopics _kafkaTopics;

        public ConsumerHostedService(IServiceProvider serviceProvider, IOptions<KafkaConfig> kafkaConfig, IOptions<KafkaTopics> kafkaTopics)
        {
            _serviceProvider = serviceProvider;
            _kafkaConfig = kafkaConfig.Value;
            _kafkaTopics = kafkaTopics.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _kafkaConfig.BootstrapServers,
                GroupId = _kafkaConfig.GroupId,
                EnableAutoCommit = _kafkaConfig.EnableAutoCommit,
                AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_kafkaConfig.AutoOffsetReset),
                AllowAutoCreateTopics = _kafkaConfig.AllowAutoCreateTopics
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig)  // Don’t register Kafka consumers in DI as singleton or scoped.
                .SetKeyDeserializer(Deserializers.Utf8)                        // Create them manually where needed, and dispose them properly.
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();

            consumer.Subscribe(_kafkaTopics.SocialMediaPostEvents);

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

                    var processedEventsRepo = scope.ServiceProvider.GetRequiredService<IProcessedEventRepository>();
                    bool isAlreadyProcessed = await processedEventsRepo.Exists(@event.Id, @event.Version);
                    if (isAlreadyProcessed)
                    {
                        // commit offset and continue — this event already applied
                        consumer.Commit(consumeResult);
                        continue;
                    }

                    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    using var transaction = await dbContext.Database.BeginTransactionAsync(token);
                    try
                    {
                        var task = (Task)handlerMethod.Invoke(eventHandler, new object[] { @event });
                        await task;

                        await processedEventsRepo.CreateAsync(new ProcessedEvent
                        {
                            AggregateId = @event.Id,
                            Version = @event.Version
                        });

                        await transaction.CommitAsync(token);

                        // Only commit Kafka offset after successful DB commit
                        consumer.Commit(consumeResult);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(token);
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ConsumerHostedService>>();
                        logger.LogError(ex, "Failed to process event {EventType} for aggregate {AggregateId} v{Version}", @event.GetType().Name, @event.Id, @event.Version);
                    }
                }
            }
        }
    }
}
