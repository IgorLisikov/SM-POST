using CQRS.Core.Commands;
using CQRS.Core.Handlers;
using MediatR;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Api.Commands
{
    public class RemoveCommentCommand : BaseCommand, IRequest
    {
        public Guid CommentId { get; set; }
        public string Username { get; set; }
    }


    public class RemoveCommentCommandHandler : IRequestHandler<RemoveCommentCommand>
    {

        private readonly IEventSourcingHandler<PostAggregate> _eventSourcingHandler;
        public RemoveCommentCommandHandler(IEventSourcingHandler<PostAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task<Unit> Handle(RemoveCommentCommand command, CancellationToken cancellation)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            aggregate.RemoveComment(command.CommentId, command.Username);
            await _eventSourcingHandler.SaveAsync(aggregate);
            return Unit.Value;
        }
    }
}
