using CQRS.Core.Commands;
using CQRS.Core.Handlers;
using MediatR;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Api.Commands
{
    public class LikePostCommand : BaseCommand, IRequest
    {
        
    }


    public class LikePostCommandHandler : IRequestHandler<LikePostCommand>
    {

        private readonly IEventSourcingHandler<PostAggregate> _eventSourcingHandler;
        public LikePostCommandHandler(IEventSourcingHandler<PostAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task<Unit> Handle(LikePostCommand command, CancellationToken cancellation)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            aggregate.LikePost();
            await _eventSourcingHandler.SaveAsync(aggregate);
            return Unit.Value;
        }
    }
}
