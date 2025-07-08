using CQRS.Core.Commands;
using CQRS.Core.Handlers;
using MediatR;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Api.Commands
{
    public class NewPostCommand : BaseCommand, IRequest
    {
        public string Author { get; set; }
        public string Message { get; set; }
    }


    public class NewPostCommandHandler : IRequestHandler<NewPostCommand>
    {

        private readonly IEventSourcingHandler<PostAggregate> _eventSourcingHandler;
        public NewPostCommandHandler(IEventSourcingHandler<PostAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task<Unit> Handle(NewPostCommand command, CancellationToken cancellation)
        {
            var aggregate = new PostAggregate(command.Id, command.Author, command.Message);
            await _eventSourcingHandler.SaveAsync(aggregate);
            return Unit.Value;
        }
    }
}
