using HBP.Common;
using HBP.Data.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HBP.Data.Postgres;

internal sealed class FluentMigrationRunner(IServiceProvider serviceProvider) : IMigrationRunner
{
    private readonly IServiceProvider _services = Guard.AgainstNull(serviceProvider);

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _services.CreateAsyncScope();
        var fluentRunner =
            scope.ServiceProvider.GetRequiredService<FluentMigrator.Runner.IMigrationRunner>();

        fluentRunner.MigrateUp();
    }
}
