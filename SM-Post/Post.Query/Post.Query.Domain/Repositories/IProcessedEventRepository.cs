using Post.Query.Domain.Entities;

namespace Post.Query.Domain.Repositories;

public interface IProcessedEventRepository
{
    Task CreateAsync(ProcessedEvent processedEvent);
    Task<bool> Exists(Guid aggregateId, int version);
}
