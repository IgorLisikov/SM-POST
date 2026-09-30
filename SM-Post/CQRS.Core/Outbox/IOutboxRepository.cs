using MongoDB.Driver;

namespace CQRS.Core.Outbox
{
    public interface IOutboxRepository
    {
        Task SaveAsync(OutboxMessage message, IClientSessionHandle session = null, CancellationToken token = default);
        Task<List<OutboxMessage>> GetUnpublishedAsync(int limit = 100, CancellationToken token = default);
        Task MarkAsPublishedAsync(Guid id, DateTime publishedAt, CancellationToken token = default);
    }
}
