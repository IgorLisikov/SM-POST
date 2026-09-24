using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using CQRS.Core.Outbox;
using MongoDB.Driver;
using Post.Cmd.Domain.Aggregates;
using System.Text.Json;

namespace Post.Cmd.Infrastructure.Stores;

public class EventStore : IEventStore
{
    private readonly IEventStoreRepository _eventStoreRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly MongoClient _mongoClient;

    public EventStore(IEventStoreRepository eventStoreRepository, IOutboxRepository outboxRepository, MongoClient mongoClient)
    {
        _eventStoreRepository = eventStoreRepository;
        _outboxRepository = outboxRepository;
        _mongoClient = mongoClient;
    }

    public async Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId)
    {
        var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);

        if (eventStream == null || !eventStream.Any())
        {
            throw new AggregateNotFoundException("Incorrect post ID provided!");
        }

        return eventStream.OrderBy(x => x.Version).Select(x => x.EventData).ToList();
    }

    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)
    {
        // optimistic concurrency control check:
        var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);
        if (expectedVersion != -1 && eventStream[^1].Version != expectedVersion)  // [^1] - takes last element in list (^ - index from the end operator)
        {
            throw new ConcurrencyException();
        }

        int version = expectedVersion;
        foreach (var @event in events)
        {
            version++;
            @event.Version = version;   // first event will have version 0
            string eventType = @event.GetType().Name;

            var eventModel = new EventModel
            {
                TimeStamp = DateTime.Now,
                AggregateIdentifier = aggregateId,
                AggregateType = nameof(PostAggregate),
                Version = version,
                EventType = eventType,
                EventData = @event
            };

            var outboxMessage = new OutboxMessage
            {
                AggregateIdentifier = aggregateId.ToString(),
                EventType = eventType,
                PayloadJson = JsonSerializer.Serialize(@event, @event.GetType()),
                OccurredOn = DateTime.UtcNow
            };

            // Transactional outbox: persist eventModel and outbox message to the same DB within the session.
            using var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();
            try
            {
                await _eventStoreRepository.SaveAsync(eventModel, session);
                // Do not call Kafka here. A separate Outbox publisher will read and publish these messages.
                await _outboxRepository.SaveAsync(outboxMessage, session);
                await session.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }
}