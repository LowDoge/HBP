namespace HBP.Common;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
