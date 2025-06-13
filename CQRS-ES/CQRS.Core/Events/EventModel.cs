using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CQRS.Core.Events;

public class EventModel  // EventModel is wrapper around event
{
    // Bson (Binary Json) - data format used by MongoDB. It is a binary representation of JSON-like documents
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }         // internal Id in MongoDB, unique
    public DateTime TimeStamp { get; set; }
    public Guid AggregateIdentifier { get; set; }   // Id of aggregate and all its events (same for each event of an aggregate)
    public string AggregateType { get; set; }
    public int Version { get; set; }                // latest version of aggregate (and event)
    public string EventType { get; set; }
    public BaseEvent EventData { get; set; }        // Concrete event
}
