using System.Data.Common;

namespace HBP.Persistence.Abstractions;

public interface IDbConnectionFactory
{
    Task<DbConnection> OpenAsync(CancellationToken cancellationToken = default);
}
