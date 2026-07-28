using FluentAssertions;
using HBP.Common;
using HBP.Persistence.Outbox;

namespace HBP.Persistence.UnitTests.Outbox;

public class OutboxMessageTests
{
    [Fact]
    public void CreateUsesEventOccurredAt()
    {
        var occurred = DateTimeOffset.UtcNow;
        var evt = new TestEvent { OccurredAt = occurred };

        var msg = OutboxMessage.Create(evt, "{}");

        msg.OccurredAt.Should().Be(occurred);
    }

    [Fact]
    public void CreateUsesFullTypeNameAsEventType()
    {
        var evt = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };

        var msg = OutboxMessage.Create(evt, "{}");

        msg.EventType.Should().Be(typeof(TestEvent).FullName);
    }

    [Fact]
    public void CreateBuildsMessageWithDefaults()
    {
        var evt = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };

        var msg = OutboxMessage.Create(evt, "{}");

        msg.ProcessedAt.Should().BeNull();
        msg.RetryCount.Should().Be(0);
        msg.LastError.Should().BeNull();
        msg.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreatePreservesPayload()
    {
        const string payload = "{\"x\":1}";
        var evt = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };

        var msg = OutboxMessage.Create(evt, payload);

        msg.Payload.Should().Be(payload);
    }

    [Fact]
    public void CreateGeneratesUniqueIds()
    {
        var evt = new TestEvent { OccurredAt = DateTimeOffset.UtcNow };

        var msg1 = OutboxMessage.Create(evt, "{}");
        var msg2 = OutboxMessage.Create(evt, "{}");

        msg1.Id.Should().NotBe(msg2.Id);
    }

    [Fact]
    public void RecordSupportsValueEquality()
    {
        var id = Guid.NewGuid();
        var occurred = DateTimeOffset.UtcNow;

        var a = new OutboxMessage(id, occurred, "Type", "{}", null, 0, null);
        var b = new OutboxMessage(id, occurred, "Type", "{}", null, 0, null);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    private sealed class TestEvent : IDomainEvent
    {
        public DateTimeOffset OccurredAt { get; init; }
    }
}