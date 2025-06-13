using CQRS.Core.Events;
using Post.Common.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Post.Query.Infrastructure.Converters
{
    // When working with a base class like BaseEvent and multiple derived classes(e.g., EventA, EventB),
    // standard JSON serializers struggle to correctly deserialize JSON into the appropriate derived class.

    // By creating a custom JsonConverter<BaseEvent>, you can implement logic to determine which derived type a given JSON object corresponds to,
    // and deserialize the object into the correct derived class.
    public class EventJsonConverter : JsonConverter<BaseEvent>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            // expects typeToConvert to be BaseEvent, because initial call is always "JsonSerializer.Deserialize<BaseEvent>()"
            return typeToConvert.IsAssignableFrom(typeof(BaseEvent));
        }

        public override BaseEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // before entering this method CanConvert() is called, typeToConvert = BaseEvent, returns true.
            if (!JsonDocument.TryParseValue(ref reader, out var doc))
            {
                throw new JsonException($"Failed to parse {nameof(JsonDocument)}!");
            }

            if (!doc.RootElement.TryGetProperty("Type", out var type))  // here JSON is treated as BaseEvent; can get the value of "Type" property
            {
                throw new JsonException("Could not detect the Type descriminator property!");
            }

            var typeDescriminator = type.GetString();    // here typeDescriminator is name of ConcreteEvent
            var json = doc.RootElement.GetRawText();

            return typeDescriminator switch
            {
                // here CanConvert() will be called for selected case, but typeToConvert = ConcreteEvent, returns false
                nameof(PostCreatedEvent) => JsonSerializer.Deserialize<PostCreatedEvent>(json, options),  // need to provide options ???
                nameof(MessageUpdatedEvent) => JsonSerializer.Deserialize<MessageUpdatedEvent>(json, options),
                nameof(PostLikedEvent) => JsonSerializer.Deserialize<PostLikedEvent>(json, options),
                nameof(CommentAddedEvent) => JsonSerializer.Deserialize<CommentAddedEvent>(json, options),
                nameof(CommentUpdatedEvent) => JsonSerializer.Deserialize<CommentUpdatedEvent>(json, options),
                nameof(CommentRemovedEvent) => JsonSerializer.Deserialize<CommentRemovedEvent>(json, options),
                nameof(PostRemovedEvent) => JsonSerializer.Deserialize<PostRemovedEvent>(json, options),
                _ => throw new JsonException($"Type descriminator is not supported yet!")
            };
        }

        public override void Write(Utf8JsonWriter writer, BaseEvent value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
