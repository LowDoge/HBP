using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using HBP.Data.Abstractions;
using Moq;

namespace HBP.Data.Postgres.UnitTests.Postgres;

public class UnitOfWorkTests
{
    [Fact]
    public void ConnectionIsNotNullAfterConstruction()
    {
        var uow = new UnitOfWork(new TestDbConnection());

        var context = (IDbContext)uow;
        context.Connection.Should().NotBeNull();
    }

    [Fact]
    public void TransactionIsNullBeforeBegin()
    {
        var uow = new UnitOfWork(new TestDbConnection());

        var context = (IDbContext)uow;
        context.Transaction.Should().BeNull();
    }

    [Fact]
    public async Task BeginAsyncStartsTransaction()
    {
        var transaction = new Mock<DbTransaction>();
        var connection = new TestDbConnection { TransactionToReturn = transaction.Object };

        var uow = new UnitOfWork(connection);
        await uow.BeginAsync();

        connection.BeginTransactionCallCount.Should().Be(1);
        var context = (IDbContext)uow;
        context.Transaction.Should().Be(transaction.Object);
    }

    [Fact]
    public async Task BeginAsyncWithSerializableIsolationLevelPropagates()
    {
        var transaction = new Mock<DbTransaction>();
        var connection = new TestDbConnection { TransactionToReturn = transaction.Object };

        var uow = new UnitOfWork(connection);
        await uow.BeginAsync(IsolationLevel.Serializable);

        connection.LastIsolationLevel.Should().Be(IsolationLevel.Serializable);
    }

    [Fact]
    public async Task BeginAsyncThrowsWhenTransactionAlreadyStarted()
    {
        var connection = new TestDbConnection
        {
            TransactionToReturn = new Mock<DbTransaction>().Object,
        };

        var uow = new UnitOfWork(connection);
        await uow.BeginAsync();

        var act = () => uow.BeginAsync().AsTask();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CommitAsyncThrowsWhenNoTransaction()
    {
        var uow = new UnitOfWork(new TestDbConnection());

        var act = () => uow.CommitAsync().AsTask();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RollbackAsyncThrowsWhenNoTransaction()
    {
        var uow = new UnitOfWork(new TestDbConnection());

        var act = () => uow.RollbackAsync().AsTask();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CommitAsyncCommitsAndDisposesTransaction()
    {
        var transaction = new Mock<DbTransaction>();
        transaction
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        transaction.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var uow = CreateUnitOfWorkWithTransaction(transaction.Object);
        await uow.CommitAsync();

        transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        transaction.Verify(x => x.DisposeAsync(), Times.Once);
        VerifyTransactionClearedAndConnectionAlive(uow);
    }

    [Fact]
    public async Task RollbackAsyncRollbacksAndDisposesTransaction()
    {
        var transaction = new Mock<DbTransaction>();
        transaction
            .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        transaction.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var uow = CreateUnitOfWorkWithTransaction(transaction.Object);
        await uow.RollbackAsync();

        transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        transaction.Verify(x => x.DisposeAsync(), Times.Once);
        VerifyTransactionClearedAndConnectionAlive(uow);
    }

    [Fact]
    public async Task CommitAsyncAllowsNewTransactionAfterCommit()
    {
        var transaction = new Mock<DbTransaction>();
        transaction
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        transaction.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var connection = new TestDbConnection
        {
            TransactionToReturn = new Mock<DbTransaction>().Object,
        };

        var uow = new UnitOfWork(connection);
        await uow.BeginAsync();
        await uow.CommitAsync();

        connection.BeginTransactionCallCount.Should().Be(1);

        await uow.BeginAsync();
        connection.BeginTransactionCallCount.Should().Be(2);
    }

    [Fact]
    public async Task DisposeAsyncDisposesTransactionAndConnection()
    {
        var transaction = new Mock<DbTransaction>();
        transaction.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var connection = new TestDbConnection { TransactionToReturn = transaction.Object };

        var uow = new UnitOfWork(connection);
        await uow.BeginAsync();
        await uow.DisposeAsync();

        transaction.Verify(x => x.DisposeAsync(), Times.Once);
        connection.DisposeCallCount.Should().Be(1);
    }

    [Fact]
    public async Task DisposeAsyncDisposesConnectionEvenWithoutTransaction()
    {
        var connection = new TestDbConnection();

        var uow = new UnitOfWork(connection);
        await uow.DisposeAsync();

        connection.DisposeCallCount.Should().Be(1);
    }

    [Fact]
    public async Task DisposeAsyncIsIdempotent()
    {
        var connection = new TestDbConnection();

        var uow = new UnitOfWork(connection);
        await uow.DisposeAsync();
        await uow.DisposeAsync();

        connection.DisposeCallCount.Should().Be(1);
    }

    [Fact]
    public void UnitOfWorkImplementsIDbContext()
    {
        var uow = new UnitOfWork(new TestDbConnection());

        uow.Should().BeAssignableTo<IDbContext>();
    }

    private static UnitOfWork CreateUnitOfWorkWithTransaction(DbTransaction transaction)
    {
        var connection = new TestDbConnection { TransactionToReturn = transaction };
        var uow = new UnitOfWork(connection);
        uow.BeginAsync().GetAwaiter().GetResult();
        return uow;
    }

    private static void VerifyTransactionClearedAndConnectionAlive(IUnitOfWork uow)
    {
        var context = (IDbContext)uow;
        context.Transaction.Should().BeNull();
        context.Connection.Should().NotBeNull();
        context.Connection.State.Should().Be(ConnectionState.Closed);
    }

    private sealed class TestDbConnection : DbConnection
    {
        public DbTransaction? TransactionToReturn { get; init; }
        public int BeginTransactionCallCount { get; private set; }
        public IsolationLevel? LastIsolationLevel { get; private set; }
        public int DisposeCallCount { get; private set; }

        [AllowNull]
        public override string ConnectionString { get; set; } = string.Empty;

        public override string Database => "test";
        public override string DataSource => "test";
        public override string ServerVersion => "test";
        public override int ConnectionTimeout => 0;
        public override ConnectionState State => ConnectionState.Closed;

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            BeginTransactionCallCount++;
            LastIsolationLevel = isolationLevel;
            return TransactionToReturn
                ?? throw new InvalidOperationException("TransactionToReturn not set");
        }

        public override ValueTask DisposeAsync()
        {
            DisposeCallCount++;
            return ValueTask.CompletedTask;
        }

        public override void Open() { }

        public override void Close() { }

        protected override DbCommand CreateDbCommand() => null!;

        public override void ChangeDatabase(string databaseName) { }
    }
}
