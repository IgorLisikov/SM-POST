using CQRS.Core.Domain;

namespace CQRS.Core.Handlers;

public interface IEventSourcingHandler<T>
{
    Task<T> GetByIdAsync(Guid aggregateId);  // T - concrete aggregate  (PostAggregate)
    Task SaveAsync(AggregateRoot aggregate);
}
