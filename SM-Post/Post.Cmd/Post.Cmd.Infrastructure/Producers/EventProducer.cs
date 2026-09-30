using Confluent.Kafka;
using CQRS.Core.Events;
using CQRS.Core.Producers;
using Microsoft.Extensions.Options;
using Post.Common.Configs;
using System.Text.Json;

namespace Post.Cmd.Infrastructure.Producers;

// Wrapper over Kafka IProducer, produces events to queue
public class EventProducer : IEventProducer
{
    private readonly ProducerConfig _producerConfig;

    public EventProducer(IOptions<KafkaConfig> kafkaConfig)
    {
        _producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaConfig.Value.BootstrapServers,
        };
    }


    public async Task ProduceAsync<T>(string topic, T @event, CancellationToken token = default) where T : BaseEvent
    {
        using var producer = new ProducerBuilder<string, string>(_producerConfig)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(Serializers.Utf8)
            .Build();

        var eventMessage = new Message<string, string>
        {
            Key = @event.Id.ToString(),
            Value = JsonSerializer.Serialize(@event, @event.GetType())
        };

        var deliveryResult = await producer.ProduceAsync(topic, eventMessage, token);

        if (deliveryResult.Status == PersistenceStatus.NotPersisted)
        {
            throw new Exception($"Could not produce {@event.GetType().Name} message to topic - {topic} due to the following reason: {deliveryResult.Message}!");
        }
    }
}
