namespace HBP.Data.Abstractions;

public interface IMigrationRunner
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
