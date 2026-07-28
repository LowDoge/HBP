using System.Reflection;
using HBP.Persistence.Abstractions;
using HBP.Persistence.Migrations;
using HBP.Persistence.Outbox;
using HBP.Persistence.Postgres;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HBP.Persistence;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddHbpPersistence(IConfiguration configuration,
                                                    string connectionName)
        {
            var connStr = configuration.GetConnectionString(connectionName)
                          ?? throw new
                              InvalidOperationException($"Connection string '{connectionName}' is not configured.");

            services.AddSingleton<IDbConnectionFactory>(_ => new PostgresConnectionFactory(connStr));
            services.AddScoped<IUnitOfWork, PostgresUnitOfWork>();
            services.AddScoped<IOutboxStore, PostgresOutboxStore>();

            return services;
        }

        public IServiceCollection AddHbpMigrations(IConfiguration configuration,
                                                   string connectionName,
                                                   params Assembly[] migrationAssemblies)
        {
            var connStr = configuration.GetConnectionString(connectionName)
                          ?? throw new
                              InvalidOperationException($"Connection string '{connectionName}' is not configured.");

            services.AddSingleton<IMigrationRunner>(sp => new FluentMigrationRunner(
                                                         connStr,
                                                         migrationAssemblies,
                                                         sp.GetRequiredService<ILogger<FluentMigrationRunner>>()));

            return services;
        }
    }
}
