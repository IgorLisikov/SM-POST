using MongoDB.Driver;

namespace CQRS.Core.Outbox
{
    public interface IOutboxRepository
    {
        Task SaveAsync(OutboxMessage message, IClientSessionHandle session = null);
        Task<List<OutboxMessage>> GetUnpublishedAsync(int limit = 100);
        Task MarkAsPublishedAsync(Guid id, DateTime publishedAt);
    }
}
