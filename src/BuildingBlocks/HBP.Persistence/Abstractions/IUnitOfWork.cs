using System.Data;
using System.Data.Common;

namespace HBP.Persistence.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    DbConnection? Connection { get; }
    DbTransaction? Transaction { get; }
    bool IsActive { get; }

    Task BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                    CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
