using CQRS.Core.Commands;
using CQRS.Core.Handlers;
using MediatR;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Api.Commands
{
    public class DeletePostCommand : BaseCommand, IRequest
    {
        public string Username { get; set; }
    }


    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
    {

        private readonly IEventSourcingHandler<PostAggregate> _eventSourcingHandler;
        public DeletePostCommandHandler(IEventSourcingHandler<PostAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task<Unit> Handle(DeletePostCommand command, CancellationToken cancellation)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            aggregate.DeletePostCommand(command.Username);
            await _eventSourcingHandler.SaveAsync(aggregate);
            return Unit.Value;
        }
    }
}
