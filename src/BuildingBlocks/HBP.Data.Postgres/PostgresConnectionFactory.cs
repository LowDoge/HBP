using System.Data.Common;
using HBP.Common;
using HBP.Data.Abstractions;
using Npgsql;

namespace HBP.Data.Postgres;

internal sealed class PostgresConnectionFactory(string connectionString) : IDbConnectionFactory
{
    private readonly string _connectionString = Guard.AgainstNullOrWhiteSpace(connectionString);

    public DbConnection Open()
    {
        var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        return conn;
    }
}
