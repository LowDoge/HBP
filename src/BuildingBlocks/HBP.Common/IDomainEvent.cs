namespace HBP.Common;

public interface IDomainEvent
{
    DateTimeOffset OccuredAt { get; }
}
