using MongoDB.Driver;

namespace CQRS.Core.Outbox
{
    public interface IOutboxRepository
    {
        Task SaveAsync(OutboxMessage message, IClientSessionHandle session = null, CancellationToken token = default);
        IAsyncEnumerable<OutboxMessage> SubscribeAsync(CancellationToken token = default);
        Task MarkAsPublishedAsync(Guid id, DateTime publishedAt, CancellationToken token = default);
    }
}
