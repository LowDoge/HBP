namespace HBP.Messaging.Kafka;

internal sealed record OutboxMessage(
    Guid Id,
    string Topic,
    string? Key,
    string Type,
    string Payload,
    DateTime CreatedAt,
    int RetryCount = 0,
    DateTime? ProcessedAt = null,
    string? Error = null
);
