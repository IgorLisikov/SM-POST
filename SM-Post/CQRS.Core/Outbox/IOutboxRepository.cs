namespace CQRS.Core.Outbox
{
    public interface IOutboxRepository
    {
        Task SaveAsync(OutboxMessage message);
        Task<List<OutboxMessage>> GetUnpublishedAsync(int limit = 100);
        Task MarkAsPublishedAsync(Guid id, DateTime publishedAt);
    }
}
