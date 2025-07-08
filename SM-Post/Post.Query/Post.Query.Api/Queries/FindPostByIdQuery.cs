using MediatR;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;

namespace Post.Query.Api.Queries;

public class FindPostByIdQuery : IRequest<List<PostEntity>>
{
    public Guid Id { get; set; }
}


public class FindPostByIdQueryHandler : IRequestHandler<FindPostByIdQuery, List<PostEntity>>
{
    private readonly IPostRepository _postRepository;
    public FindPostByIdQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<PostEntity>> Handle(FindPostByIdQuery query, CancellationToken cancellation)
    {
        var post = await _postRepository.GetByIdAsync(query.Id);
        return new List<PostEntity> { post };
    }
}
