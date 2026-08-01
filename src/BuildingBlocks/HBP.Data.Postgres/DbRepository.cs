using System.Data.Common;
using Dapper;
using HBP.Common;
using HBP.Data.Abstractions;

namespace HBP.Data.Postgres;

public abstract class DbRepository(IDbContext dbContext)
{
    private readonly IDbContext _dbContext = Guard.AgainstNull(dbContext);

    protected DbConnection Connection => _dbContext.Connection;
    protected DbTransaction? Transaction => _dbContext.Transaction;

    protected CommandDefinition CreateCommand(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        return new CommandDefinition(
            sql,
            parameters,
            Transaction,
            cancellationToken: cancellationToken
        );
    }

    protected Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        return Connection.ExecuteAsync(CreateCommand(sql, parameters, cancellationToken));
    }

    protected Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        return Connection.QuerySingleOrDefaultAsync<T>(
            CreateCommand(sql, parameters, cancellationToken)
        );
    }

    protected Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        return Connection.QueryAsync<T>(CreateCommand(sql, parameters, cancellationToken));
    }
}
