using FluentAssertions;
using HBP.Common;
using HBP.Persistence.Abstractions;
using HBP.Persistence.Postgres;
using Moq;

namespace HBP.Persistence.UnitTests.Outbox;

public class PostgresOutboxStoreTests
{
    [Fact]
    public void ConstructorThrowsForNullUnitOfWork()
    {
        var act = () => new PostgresOutboxStore(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("unitOfWork");
    }

    [Fact]
    public async Task AddAsyncThrowsForNullEvent()
    {
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.IsActive).Returns(true);
        var store = new PostgresOutboxStore(uow.Object);

        var act = () => store.AddAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddAsyncThrowsWhenUnitOfWorkNotActive()
    {
        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.IsActive).Returns(false);
        var store = new PostgresOutboxStore(uow.Object);
        var evt = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };

        var act = () => store.AddAsync(evt);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*active unit of work*");

        uow.VerifyGet(x => x.Connection, Times.Never);
    }

    private sealed class TestEvent : IDomainEvent
    {
        public DateTimeOffset OccurredAt { get; init; }
    }
}