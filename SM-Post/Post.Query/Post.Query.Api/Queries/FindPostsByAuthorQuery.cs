using MediatR;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;

namespace Post.Query.Api.Queries;

public class FindPostsByAuthorQuery : IRequest<List<PostEntity>>
{
    public string Author { get; set; }
}


public class FindPostsByAuthorQueryHandler : IRequestHandler<FindPostsByAuthorQuery, List<PostEntity>>
{
    private readonly IPostRepository _postRepository;
    public FindPostsByAuthorQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<PostEntity>> Handle(FindPostsByAuthorQuery query, CancellationToken cancellation)
    {
        return await _postRepository.ListByAuthorAsync(query.Author);
    }
}