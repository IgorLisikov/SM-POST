using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;

namespace Post.Query.Infrastructure.Repositories
{
    public class ProcessedEventRepository : IProcessedEventRepository
    {
        private readonly DatabaseContext _context;

        public ProcessedEventRepository(DatabaseContext context)
        {
            _context = context;
        }


        public async Task CreateAsync(ProcessedEvent processedEvent, CancellationToken token = default)
        {
            _context.ProcessedEvents.Add(processedEvent);
            await _context.SaveChangesAsync(token);
        }

        public async Task<bool> Exists(Guid aggregateId, int version, CancellationToken token = default)
        {
            return await _context.ProcessedEvents
                .AnyAsync(x => x.AggregateId == aggregateId && x.Version == version, token);
        }
    }
}
