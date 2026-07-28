using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using HBP.Persistence.Abstractions;
using HBP.Persistence.Postgres;
using Moq;

namespace HBP.Persistence.UnitTests.Postgres;

public class PostgresUnitOfWorkTests
{
    [Fact]
    public void ConstructorThrowsForNullConnectionFactory()
    {
        var act = () => new PostgresUnitOfWork(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("connectionFactory");
    }

    [Fact]
    public void ConnectionPropertyIsNullBeforeBegin()
    {
        var uow = CreateUnitOfWork();

        uow.Connection.Should().BeNull();
    }

    [Fact]
    public void IsActiveIsFalseBeforeBegin()
    {
        var uow = CreateUnitOfWork();

        uow.IsActive.Should().BeFalse();
        uow.Transaction.Should().BeNull();
        uow.Connection.Should().BeNull();
    }

    [Fact]
    public async Task BeginAsyncOpensConnectionAndStartsTransaction()
    {
        var transaction = new Mock<DbTransaction>();
        var connection = new TestDbConnection { TransactionToReturn = transaction.Object };
        var factory = new Mock<IDbConnectionFactory>();
        factory
            .Setup(x => x.OpenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);

        var uow = new PostgresUnitOfWork(factory.Object);

        await uow.BeginAsync();

        factory.Verify(x => x.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);
        connection.BeginTransactionCallCount.Should().Be(1);
        connection.LastIsolationLevel.Should().Be(IsolationLevel.ReadCommitted);
        uow.IsActive.Should().BeTrue();
        uow.Transaction.Should().NotBeNull();
        uow.Connection.Should().NotBeNull();
    }

    [Fact]
    public async Task BeginAsyncWithSerializableIsolationLevelPropagates()
    {
        var transaction = new Mock<DbTransaction>();
        var connection = new TestDbConnection { TransactionToReturn = transaction.Object };
        var factory = new Mock<IDbConnectionFactory>();
        factory
            .Setup(x => x.OpenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);

        var uow = new PostgresUnitOfWork(factory.Object);

        await uow.BeginAsync(IsolationLevel.Serializable);

        connection.LastIsolationLevel.Should().Be(IsolationLevel.Serializable);
    }

    [Fact]
    public async Task BeginAsyncThrowsWhenAlreadyActive()
    {
        var transaction = new Mock<DbTransaction>();
        var uow = CreateActiveUnitOfWork(transaction.Object);

        var act = () => uow.BeginAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already active*");
    }

    [Fact]
    public async Task CommitAsyncThrowsWhenNotActive()
    {
        var uow = CreateUnitOfWork();

        var act = () => uow.CommitAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task RollbackAsyncThrowsWhenNotActive()
    {
        var uow = CreateUnitOfWork();

        var act = () => uow.RollbackAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task CommitAsyncCommitsAndDisposes()
    {
        var transaction = new Mock<DbTransaction>();
        transaction
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        transaction
            .Setup(x => x.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var uow = CreateActiveUnitOfWork(transaction.Object);

        await uow.CommitAsync();

        transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        transaction.Verify(x => x.DisposeAsync(), Times.Once);
        uow.IsActive.Should().BeFalse();
        uow.Transaction.Should().BeNull();
        uow.Connection.Should().BeNull();
    }

    [Fact]
    public async Task RollbackAsyncRollbacksAndDisposes()
    {
        var transaction = new Mock<DbTransaction>();
        transaction
            .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        transaction
            .Setup(x => x.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var uow = CreateActiveUnitOfWork(transaction.Object);

        await uow.RollbackAsync();

        transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        transaction.Verify(x => x.DisposeAsync(), Times.Once);
        uow.IsActive.Should().BeFalse();
        uow.Transaction.Should().BeNull();
        uow.Connection.Should().BeNull();
    }

    [Fact]
    public async Task DisposeAsyncDisposesActiveConnectionAndTransaction()
    {
        var transaction = new Mock<DbTransaction>();
        transaction
            .Setup(x => x.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var connection = new TestDbConnection
        {
            TransactionToReturn = transaction.Object
        };

        var factory = new Mock<IDbConnectionFactory>();
        factory
            .Setup(x => x.OpenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);

        var uow = new PostgresUnitOfWork(factory.Object);
        await uow.BeginAsync();

        await uow.DisposeAsync();

        transaction.Verify(x => x.DisposeAsync(), Times.Once);
        connection.DisposeCallCount.Should().Be(1);
    }

    [Fact]
    public async Task DisposeAsyncIsIdempotent()
    {
        var uow = CreateUnitOfWork();

        await uow.DisposeAsync();
        await uow.DisposeAsync();
    }

    private static PostgresUnitOfWork CreateUnitOfWork()
    {
        var factory = new Mock<IDbConnectionFactory>();
        return new PostgresUnitOfWork(factory.Object);
    }

    private static PostgresUnitOfWork CreateActiveUnitOfWork(DbTransaction? transaction = null)
    {
        var txn = transaction ?? new Mock<DbTransaction>().Object;
        var connection = new TestDbConnection { TransactionToReturn = txn };

        var factory = new Mock<IDbConnectionFactory>();
        factory
            .Setup(x => x.OpenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);

        var uow = new PostgresUnitOfWork(factory.Object);
        uow.BeginAsync().GetAwaiter().GetResult();
        return uow;
    }

    private sealed class TestDbConnection : DbConnection
    {
        public DbTransaction? TransactionToReturn { get; init; }

        public int BeginTransactionCallCount { get; private set; }

        public IsolationLevel? LastIsolationLevel { get; private set; }

        public int DisposeCallCount { get; private set; }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            BeginTransactionCallCount++;
            LastIsolationLevel = isolationLevel;
            return TransactionToReturn ?? throw new InvalidOperationException("TransactionToReturn not set");
        }

        public override ValueTask DisposeAsync()
        {
            DisposeCallCount++;
            return ValueTask.CompletedTask;
        }

        [AllowNull]
        public override string ConnectionString { get; set; } = string.Empty;

        public override string Database => "test";
        public override string DataSource => "test";
        public override string ServerVersion => "test";
        public override int ConnectionTimeout => 0;
        public override ConnectionState State => ConnectionState.Closed;

        public override void Open() { }
        public override void Close() { }
        protected override DbCommand CreateDbCommand() => null!;
        public override void ChangeDatabase(string databaseName) { }
    }
}