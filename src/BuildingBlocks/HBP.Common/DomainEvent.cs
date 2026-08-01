namespace HBP.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

public abstract record DomainEvent(DateTimeOffset OccurredAt) : IDomainEvent;
