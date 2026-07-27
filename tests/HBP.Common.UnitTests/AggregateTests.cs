using FluentAssertions;

namespace HBP.Common.UnitTests;

public class AggregateTests
{
    [Fact]
    public void AddDomainEventAppendsToCollection()
    {
        var e = new TestEvent { OccuredAt = DateTimeOffset.UtcNow };
        var a = new TestAggregate { Id = Guid.NewGuid() };

        a.AddDomainEvent(e);
        a.DomainEvents.Should().HaveCount(1);
        a.DomainEvents[0].Should().BeSameAs(e);
    }

    [Fact]
    public void AddDomainEventPreservesOrder()
    {
        var e1 = new TestEvent { OccuredAt = DateTimeOffset.UtcNow };
        var e2 = new TestEvent { OccuredAt = DateTimeOffset.UtcNow };
        var e3 = new TestEvent { OccuredAt = DateTimeOffset.UtcNow };
        var a = new TestAggregate { Id = Guid.NewGuid() };

        a.AddDomainEvent(e1);
        a.AddDomainEvent(e2);
        a.AddDomainEvent(e3);

        a.DomainEvents.Should().ContainInOrder(e1, e2, e3);
    }

    [Fact]
    public void ClearDomainEventsRemovesAll()
    {
        var e1 = new TestEvent { OccuredAt = DateTimeOffset.UtcNow };
        var e2 = new TestEvent { OccuredAt = DateTimeOffset.UtcNow };
        var a = new TestAggregate { Id = Guid.NewGuid() };

        a.AddDomainEvent(e1);
        a.AddDomainEvent(e2);
        a.ClearDomainEvents();

        a.DomainEvents.Should().BeEmpty();
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public new void AddDomainEvent(IDomainEvent @event) => base.AddDomainEvent(@event);
    }

    private sealed class TestEvent : IDomainEvent
    {
        public DateTimeOffset OccuredAt { get; init; }
    }
}
