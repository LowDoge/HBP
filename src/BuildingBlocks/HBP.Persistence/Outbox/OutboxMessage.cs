using HBP.Common;

namespace HBP.Persistence.Outbox;

public sealed record OutboxMessage(
    Guid Id,
    DateTimeOffset OccurredAt,
    string EventType,
    string Payload,
    DateTimeOffset? ProcessedAt,
    int RetryCount,
    string? LastError)
{
    public static OutboxMessage Create(IDomainEvent @event, string payload) =>
        new(Id: Guid.NewGuid(),
            OccurredAt: @event.OccurredAt,
            EventType: @event.GetType().FullName ?? @event.GetType().Name,
            Payload: payload,
            ProcessedAt: null,
            RetryCount: 0,
            LastError: null);
}
