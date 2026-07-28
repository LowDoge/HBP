using System.Data;
using System.Data.Common;
using HBP.Common;
using HBP.Persistence.Abstractions;

namespace HBP.Persistence.Postgres;

internal sealed class PostgresUnitOfWork(IDbConnectionFactory connectionFactory) : IUnitOfWork
{
    private readonly IDbConnectionFactory _connectionFactory =
        Guard.AgainstNull(connectionFactory, nameof(connectionFactory));

    private bool _isDisposed;

    public DbConnection? Connection { get; private set; }

    public DbTransaction? Transaction { get; private set; }

    public bool IsActive => Connection != null && Transaction != null;

    public async Task BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                 CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (IsActive)
        {
            throw new
                InvalidOperationException("Unit of work is already active. Commit or rollback current transaction first.");
        }

        Connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        Transaction = await Connection.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!IsActive)
        {
            throw new InvalidOperationException("Unit of work is not active. Begin transaction first.");
        }

        await Transaction!.CommitAsync(cancellationToken).ConfigureAwait(false);
        await DisposeConnectionAndTransactionAsync();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!IsActive)
        {
            throw new InvalidOperationException("Unit of work is not active. Begin transaction first.");
        }

        await Transaction!.RollbackAsync(cancellationToken).ConfigureAwait(false);
        await DisposeConnectionAndTransactionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        await DisposeConnectionAndTransactionAsync();
        _isDisposed = true;
    }

    private async ValueTask DisposeConnectionAndTransactionAsync()
    {
        if (Transaction != null)
        {
            await Transaction.DisposeAsync().ConfigureAwait(false);
            Transaction = null;
        }

        if (Connection != null)
        {
            await Connection.DisposeAsync().ConfigureAwait(false);
            Connection = null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException("Unit of work disposed");
        }
    }
}
