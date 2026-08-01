namespace HBP.Messaging.Kafka;

internal interface IOutboxMessageRepository
{
    Task<IReadOnlyList<OutboxMessage>> ListUnprocessedAsync(
        int batchSize,
        CancellationToken cancellationToken = default
    );

    Task CompleteAsync(
        Guid messageId,
        DateTime processedAt,
        CancellationToken cancellationToken = default
    );

    Task FailAsync(
        Guid messageId,
        string errorMessage,
        CancellationToken cancellationToken = default
    );

    Task DeadLetterAsync(
        OutboxMessage message,
        string errorMessage,
        DateTime deadLetteredAt,
        CancellationToken cancellationToken = default
    );
}
