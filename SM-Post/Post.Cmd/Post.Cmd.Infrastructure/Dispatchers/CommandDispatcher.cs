using CQRS.Core.Commands;  // depends only on BaseCommand
using CQRS.Core.Infrastructure;

namespace Post.Cmd.Infrastructure.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher   // Mediator
    {
        // In _handlers Dictionary all HandleAsync() methods of CommandHandler class are stored.
        // This allows for CommandDispatcher to be decoupled from CommandHandler, but still be able to call its methods.
        private readonly Dictionary<Type, Func<BaseCommand, Task>> _handlers = new();

        public void RegisterHandler<T>(Func<T, Task> handler) where T : BaseCommand
        {
            if (_handlers.ContainsKey(typeof(T)))
            {
                throw new IndexOutOfRangeException("You cannot register the same command twice");
            }

            _handlers.Add(typeof(T), x => handler((T)x));
            // Each hanlder expects concrete command as parameter. But in _handlers dictionary all methods are stored as "Method(BaseCommand)".
            // Thats why it is needed to add LambdaWrapper that converts command to ConcreteCommand, and then calls handler method.
            // Here is what really gets added to dictionary as values: 
            // Task LambdaWrapper(BaseCommand command)
            // {
            //    return handler((ConcreteCommand)command);
            // }
        }

        public async Task SendAsync(BaseCommand command)
        {
            // here CommandDispatcher can call any HandleAsync() method of CommandHandler, depending only on BaseCommand
            if (_handlers.TryGetValue(command.GetType(), out Func<BaseCommand, Task> handler))
            {
                await handler(command);
            }
            else
            {
                throw new ArgumentNullException(nameof(handler), "No command handler was registered!");
            }
        }
    }
}
