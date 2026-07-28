using System.Reflection;
using FluentMigrator.Runner;
using HBP.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HBP.Persistence.Migrations;

internal sealed class FluentMigrationRunner(
    string connectionString,
    IEnumerable<Assembly> migrationAssemblies,
    ILogger<FluentMigrationRunner> logger) : IMigrationRunner
{
    private readonly string _connectionString = Guard.AgainstNullOrEmpty(connectionString, nameof(connectionString));

    private readonly Assembly[] _migrationAssemblies =
        Guard.AgainstNull(migrationAssemblies, nameof(migrationAssemblies)).ToArray();

    private readonly ILogger<FluentMigrationRunner> _logger = Guard.AgainstNull(logger, nameof(logger));

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run(() =>
        {
            using var serviceProvider = new ServiceCollection()
                                        .AddFluentMigratorCore()
                                        .ConfigureRunner(c =>
                                                             c.AddPostgres()
                                                              .WithGlobalConnectionString(_connectionString)
                                                              .ScanIn(_migrationAssemblies).For.Migrations())
                                        .BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<FluentMigrator.Runner.IMigrationRunner>();

            _logger.LogInformation("Applying migrations from {AssemblyCount} assemblies to {Connection}",
                                   _migrationAssemblies.Length, _connectionString);

            try
            {
                runner.MigrateUp();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration failed for {Connection}", _connectionString);
                throw;
            }

            _logger.LogInformation("Migrations applied successfully");
        }, cancellationToken);
    }
}
