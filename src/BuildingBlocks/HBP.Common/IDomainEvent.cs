namespace HBP.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
