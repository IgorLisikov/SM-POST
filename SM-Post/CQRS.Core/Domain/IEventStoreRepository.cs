using CQRS.Core.Events;
using MongoDB.Driver;

namespace CQRS.Core.Domain;

public interface IEventStoreRepository
{
    Task SaveAsync(EventModel @event, IClientSessionHandle session = null);
    Task<List<EventModel>> FindByAggregateId(Guid aggregateId);
}
