namespace HBP.Persistence.Migrations;

public interface IMigrationRunner
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
