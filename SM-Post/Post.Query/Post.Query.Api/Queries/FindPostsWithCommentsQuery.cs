using MediatR;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;

namespace Post.Query.Api.Queries;

public class FindPostsWithCommentsQuery : IRequest<List<PostEntity>>
{

}


public class FindPostsWithCommentsQueryHandler : IRequestHandler<FindPostsWithCommentsQuery, List<PostEntity>>
{
    private readonly IPostRepository _postRepository;
    public FindPostsWithCommentsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<PostEntity>> Handle(FindPostsWithCommentsQuery query, CancellationToken cancellation)
    {
        return await _postRepository.ListWithCommentsAsync();
    }
}
