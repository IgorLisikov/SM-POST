using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;

namespace Post.Query.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DatabaseContextFactory _contextFactory;

        public CommentRepository(DatabaseContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Context is created on every request because repository methods are called by HostedService
        // Explanation:
        // Scoped DbContext in Web Requests vs.Background Services:
        // In ASP.NET Core, DbContext is typically registered with a scoped lifetime (builder.Services.AddDbContext<T>()).
        // However, background services(IHostedService) do not have a request scope, so they cannot use a scoped DbContext.
        public async Task CreateAsync(CommentEntity comment)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Comments.Add(comment);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid commentId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var comment = await GetByIdAsync(commentId);
                if (comment == null) return;

                context.Comments.Remove(comment);
                await context.SaveChangesAsync();
            }
        }

        public async Task<CommentEntity> GetByIdAsync(Guid commentId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.Comments
                    .FirstOrDefaultAsync(x => x.CommentId == commentId);
            }
        }

        public async Task UpdateAsync(CommentEntity comment)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Comments.Update(comment);
                await context.SaveChangesAsync();
            }
        }
    }
}
