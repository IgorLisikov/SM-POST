using MediatR;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;

namespace Post.Query.Api.Queries;

public class FindAllPostsQuery : IRequest<List<PostEntity>>
{

}


public class FindAllPostsQueryHandler : IRequestHandler<FindAllPostsQuery, List<PostEntity>>
{
    private readonly IPostRepository _postRepository;
    public FindAllPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<PostEntity>> Handle(FindAllPostsQuery query, CancellationToken cancellation)
    {
        return await _postRepository.ListAllAsync();
    }
}
