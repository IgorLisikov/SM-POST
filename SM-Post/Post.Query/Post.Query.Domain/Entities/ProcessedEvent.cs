namespace Post.Query.Domain.Entities
{
    public class ProcessedEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AggregateId { get; set; }
        public int Version { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
