using CQRS.Core.Commands;
using CQRS.Core.Handlers;
using MediatR;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Api.Commands
{
    public class EditMessageCommand : BaseCommand, IRequest
    {
        public string Message { get; set; }
    }


    public class EditMessageCommandHandler : IRequestHandler<EditMessageCommand>
    {

        private readonly IEventSourcingHandler<PostAggregate> _eventSourcingHandler;
        public EditMessageCommandHandler(IEventSourcingHandler<PostAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task<Unit> Handle(EditMessageCommand command, CancellationToken cancellation)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            aggregate.EditMessage(command.Message);
            await _eventSourcingHandler.SaveAsync(aggregate);
            return Unit.Value;
        }
    }
}
