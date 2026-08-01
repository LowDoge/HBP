using FluentAssertions;

namespace HBP.Common.UnitTests;

public class AggregateTests
{
    [Fact]
    public void AddDomainEventAppendsToCollection()
    {
        var e = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };
        var a = new TestAggregate(Guid.NewGuid());

        a.AddDomainEvent(e);
        a.DomainEvents.Should().HaveCount(1);
        a.DomainEvents[0].Should().BeSameAs(e);
    }

    [Fact]
    public void AddDomainEventPreservesOrder()
    {
        var e1 = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };
        var e2 = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };
        var e3 = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };
        var a = new TestAggregate(Guid.NewGuid());

        a.AddDomainEvent(e1);
        a.AddDomainEvent(e2);
        a.AddDomainEvent(e3);

        a.DomainEvents.Should().ContainInOrder(e1, e2, e3);
    }

    [Fact]
    public void ClearDomainEventsRemovesAll()
    {
        var e1 = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };
        var e2 = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };
        var a = new TestAggregate(Guid.NewGuid());

        a.AddDomainEvent(e1);
        a.AddDomainEvent(e2);
        a.ClearDomainEvents();

        a.DomainEvents.Should().BeEmpty();
    }

    private sealed class TestAggregate(Guid id) : AggregateRoot<Guid>(id)
    {
        public new void AddDomainEvent(IDomainEvent @event) => base.AddDomainEvent(@event);
    }

    private sealed class TestEvent : IDomainEvent
    {
        public DateTimeOffset OccurredAt { get; init; }
    }
}
