namespace CQRS.Core.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
        public string AggregateIdentifier { get; set; }
        public string EventType { get; set; }
        public string PayloadJson { get; set; }      // serialized event JSON
        public bool Published { get; set; } = false;
        public DateTime? PublishedAt { get; set; }
    }
}
