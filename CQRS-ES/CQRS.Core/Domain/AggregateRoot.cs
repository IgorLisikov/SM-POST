using CQRS.Core.Events;

namespace CQRS.Core.Domain;

public abstract class AggregateRoot
{
    protected Guid _id;  // all events related to this aggregate will have the same Id as the aggregate Id

    public Guid Id
    {
        get { return _id; }
    }


    private readonly List<BaseEvent> _changes = new();  // list of uncommited events


    public int Version { get; set; } = -1;   // corresponds to version of latest event, first event will have version 0

    public IEnumerable<BaseEvent> GetUncommitedChanges()
    {
        return _changes;
    }

    public void MarkChangesAsCommited()
    {
        _changes.Clear();
    }


    // The goal of ApplyChange() is to get corresponging Apply() method of PostAggregate and call it.
    // Also it saves @event to list of uncommited events - _changes.
    private void ApplyChange(BaseEvent @event, bool isNew)
    {
        // 'this' below is concrete aggregate
        var method = this.GetType().GetMethod("Apply", new Type[] { @event.GetType() });  // get specific 'Apply' method that takes ConcreteEvent as parameter

        if (method == null)
        {
            throw new ArgumentNullException(nameof(method), $"The Apply method was not found in the Aggregate for {@event.GetType().Name}!");
        }

        method.Invoke(this, new object[] { @event });  // results is PostAggregateInstance.Apply(@event);

        if (isNew)
        {
            _changes.Add(@event);
        }
    }

    protected void RaiseEvent(BaseEvent @event)  // Raising an event and ReplayEvents is the way to change aggregate or "Apply change to aggregate"
    {
        ApplyChange(@event, true);
    }

    public void ReplayEvents(IEnumerable<BaseEvent> events)
    {
        foreach (var @event in events)
        {
            ApplyChange(@event, false);
        }
    }
}
