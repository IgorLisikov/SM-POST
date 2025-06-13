using CQRS.Core.Messages;

namespace CQRS.Core.Events
{
    public abstract class BaseEvent : Message
    {
        protected BaseEvent(string type)
        {
            Type = type;
        }
        
        public int Version { get; set; }  // corresponds to version of aggregate
        public string Type { get; set; }
    }
}
