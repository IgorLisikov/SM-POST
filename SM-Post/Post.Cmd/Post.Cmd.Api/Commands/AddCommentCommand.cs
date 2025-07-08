using CQRS.Core.Commands;
using CQRS.Core.Handlers;
using MediatR;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Api.Commands
{
    public class AddCommentCommand : BaseCommand, IRequest
    {
        public string Comment { get; set; }
        public string Username { get; set; }
    }

    public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand>
    {

        private readonly IEventSourcingHandler<PostAggregate> _eventSourcingHandler;
        public AddCommentCommandHandler(IEventSourcingHandler<PostAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task<Unit> Handle(AddCommentCommand command, CancellationToken cancellation)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            aggregate.AddComment(command.Comment, command.Username);
            await _eventSourcingHandler.SaveAsync(aggregate);
            return Unit.Value;
        }
    }
}

