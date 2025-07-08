using MediatR;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;

namespace Post.Query.Api.Queries;

public class FindPostsWithLikesQuery : IRequest<List<PostEntity>>
{
    public int NumberOfLikes { get; set; }
}

public class FindPostsWithLikesQueryHandler : IRequestHandler<FindPostsWithLikesQuery, List<PostEntity>>
{
    private readonly IPostRepository _postRepository;
    public FindPostsWithLikesQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<PostEntity>> Handle(FindPostsWithLikesQuery query, CancellationToken cancellation)
    {
        return await _postRepository.ListWithLikesAsync(query.NumberOfLikes);
    }
}
