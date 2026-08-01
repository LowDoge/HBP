using System.Data.Common;
using System.Reflection;
using FluentMigrator.Runner;
using HBP.Data.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IMigrationRunner = HBP.Data.Abstractions.IMigrationRunner;

namespace HBP.Data.Postgres;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHbpData(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionName,
        params Assembly[] migrationAssemblies
    )
    {
        // Database
        services.AddSingleton<IDbConnectionFactory>(sp =>
        {
            var connString =
                configuration.GetConnectionString(connectionName)
                ?? throw new InvalidOperationException(
                    $"Connection '{connectionName}' is not configured."
                );
            return new PostgresConnectionFactory(connString);
        });
        services.AddScoped<DbConnection>(sp =>
            sp.GetRequiredService<IDbConnectionFactory>().Open()
        );
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<UnitOfWork>());

        // Migrator
        services.AddSingleton<IMigrationRunner, FluentMigrationRunner>();
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(c =>
                c.AddPostgres()
                    .WithGlobalConnectionString(connectionName)
                    .ScanIn(migrationAssemblies)
                    .For.Migrations()
            );

        return services;
    }
}
