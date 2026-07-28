using System.Data;

namespace HBP.Persistence.Abstractions;

public interface IDbConnectionFactory
{
    Task<IDbConnection> OpenAsync(CancellationToken cancellationToken = default);
}
