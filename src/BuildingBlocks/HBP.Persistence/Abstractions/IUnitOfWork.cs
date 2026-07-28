using System.Data;

namespace HBP.Persistence.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }
    bool IsActive { get; }

    Task BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                    CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
