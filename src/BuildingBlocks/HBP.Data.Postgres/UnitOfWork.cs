using System.Data;
using System.Data.Common;
using HBP.Common;
using HBP.Data.Abstractions;

namespace HBP.Data.Postgres;

internal sealed class UnitOfWork(DbConnection connection)
    : IUnitOfWork,
        IDbContext,
        IAsyncDisposable
{
    private bool _isDisposed;

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        if (Transaction != null)
        {
            await Transaction.DisposeAsync().ConfigureAwait(false);
            Transaction = null;
        }

        await Connection.DisposeAsync().ConfigureAwait(false);
        Connection = null!;

        _isDisposed = true;

        // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
        GC.SuppressFinalize(this);
    }

    public DbConnection Connection { get; private set; } = Guard.AgainstNull(connection);
    public DbTransaction? Transaction { get; private set; }

    public async ValueTask BeginAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default
    )
    {
        ThrowIfDisposed();

        if (Transaction != null)
        {
            throw new InvalidOperationException("Transaction is already started.");
        }

        Transaction = await Connection
            .BeginTransactionAsync(isolationLevel, cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask CommitAsync(CancellationToken cancellationToken = default)
    {
        if (Transaction == null)
        {
            throw new InvalidOperationException("Transaction is not started yet.");
        }

        await Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        await Transaction.DisposeAsync();
        Transaction = null;
    }

    public async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (Transaction == null)
        {
            throw new InvalidOperationException("Transaction is not started yet.");
        }

        await Transaction.RollbackAsync(cancellationToken);
        await Transaction.DisposeAsync();
        Transaction = null;
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }
    }
}
