using System.Data.Common;
using HBP.Common;
using HBP.Persistence.Abstractions;
using Npgsql;

namespace HBP.Persistence.Postgres;

internal class PostgresConnectionFactory(string connectionString) : IDbConnectionFactory
{
    private readonly string _connectionString = Guard.AgainstNullOrEmpty(connectionString, nameof(connectionString));

    public async Task<DbConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        var conn = new NpgsqlConnection(_connectionString);
        // TODO: Поресёрчить юзабилити ConfigureAwait в .NET ~7 и выше, по умолчанию SynchronizationContext.Current == null,
        // что приводит к рудиментарности ConfigureAwait
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        return conn;
    }
}
