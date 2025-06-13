namespace CQRS.Core.Messages
{
    public abstract class Message
    {
        public Guid Id { get; set; }  // represents Id of related aggregate
    }
}
