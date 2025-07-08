using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;

namespace Post.Query.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly DatabaseContext _context;

        public PostRepository(DatabaseContext context)
        {
            _context = context;
        }


        public async Task CreateAsync(PostEntity post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid postId)
        {
            var post = await GetByIdAsync(postId);
            if (post == null) return;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        public async Task<PostEntity> GetByIdAsync(Guid postId)
        {
            return await _context.Posts
                .Include(x => x.Comments)
                .FirstOrDefaultAsync(x => x.PostId == postId);
        }

        public async Task<List<PostEntity>> ListAllAsync()
        {
            return await _context.Posts.AsNoTracking()    // suppose the list is used in readonly fashion
                .Include(x => x.Comments).AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PostEntity>> ListByAuthorAsync(string author)
        {
            return await _context.Posts.AsNoTracking()
                .Include(x => x.Comments).AsNoTracking()
                .Where(x => x.Author.Contains(author))
                .ToListAsync();
        }

        public async Task<List<PostEntity>> ListWithCommentsAsync()
        {
            return await _context.Posts.AsNoTracking()
                .Include(x => x.Comments).AsNoTracking()
                .Where(x => x.Comments != null && x.Comments.Any())
                .ToListAsync();
        }

        public async Task<List<PostEntity>> ListWithLikesAsync(int numberOfLikes)
        {
            return await _context.Posts.AsNoTracking()
                .Include(x => x.Comments).AsNoTracking()
                .Where(x => x.Likes >= numberOfLikes)
                .ToListAsync();
        }

        public async Task UpdateAsync(PostEntity post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }
    }
}
