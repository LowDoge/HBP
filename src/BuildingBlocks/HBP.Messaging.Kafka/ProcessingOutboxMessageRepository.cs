using System.Data.Common;
using Dapper;
using HBP.Common;

namespace HBP.Messaging.Kafka;

internal sealed class ProcessingOutboxMessageRepository(DbConnection connection)
    : IOutboxMessageRepository
{
    private const string ListUnprocessedSql = """
        SELECT  id              AS "ID",
                topic           AS "Topic",
                key             AS "Key",
                type            AS "Type",
                payload         AS "Payload",
                created_at      AS "CreatedAt",
                retry_count     AS "RetryCount",
                processed_at    AS "ProcessedAt",
                error           AS "Error"
        FROM outbox_messages
        WHERE processed_at IS NULL
        ORDER BY created_at
        LIMIT @BatchSize
        """;

    private const string CompleteSql = """
        UPDATE outbox_messages
        SET processed_at = @ProcessedAt,
            error = NULL
        WHERE id = @Id
        """;

    private const string FailSql = """
        UPDATE outbox_messages
        SET retry_count = retry_count + 1,
            error = @Error
        WHERE id = @Id
        """;

    private const string DeadLetterSql = """
        INSERT INTO outbox_dead_letter_messages (id, topic, key, type, payload, retry_count, error, dead_lettered_at)
        VALUES (@Id, @Topic, @Key, @Type, @Payload::jsonb, @RetryCount, @Error, @DeadLetteredAt)
        """;

    private const string MarkProcessedSql = """
        UPDATE outbox_messages
        SET processed_at = @ProcessedAt,
            error = @Error
        WHERE id = @Id
        """;

    private readonly DbConnection _connection = Guard.AgainstNull(connection);

    public async Task<IReadOnlyList<OutboxMessage>> ListUnprocessedAsync(
        int batchSize,
        CancellationToken cancellationToken = default
    )
    {
        Guard.AgainstNegative(batchSize);

        if (batchSize == 0)
        {
            return Array.Empty<OutboxMessage>();
        }

        return (
            await _connection.QueryAsync<OutboxMessage>(
                new CommandDefinition(
                    ListUnprocessedSql,
                    new { BatchSize = batchSize },
                    cancellationToken: cancellationToken
                )
            )
        ).AsList();
    }

    public Task CompleteAsync(
        Guid messageId,
        DateTime processedAt,
        CancellationToken cancellationToken = default
    )
    {
        return _connection.ExecuteAsync(
            new CommandDefinition(
                CompleteSql,
                new { Id = messageId, ProcessedAt = processedAt },
                cancellationToken: cancellationToken
            )
        );
    }

    public Task FailAsync(
        Guid messageId,
        string errorMessage,
        CancellationToken cancellationToken = default
    )
    {
        return _connection.ExecuteAsync(
            new CommandDefinition(
                FailSql,
                new { Id = messageId, Error = errorMessage },
                cancellationToken: cancellationToken
            )
        );
    }

    public async Task DeadLetterAsync(
        OutboxMessage message,
        string errorMessage,
        DateTime deadLetteredAt,
        CancellationToken cancellationToken = default
    )
    {
        Guard.AgainstNull(message);

        await _connection
            .ExecuteAsync(
                new CommandDefinition(
                    DeadLetterSql,
                    new
                    {
                        message.Id,
                        message.Topic,
                        message.Key,
                        message.Type,
                        message.Payload,
                        RetryCount = message.RetryCount + 1,
                        Error = errorMessage,
                        DeadLetteredAt = deadLetteredAt,
                    },
                    cancellationToken: cancellationToken
                )
            )
            .ConfigureAwait(false);

        await _connection
            .ExecuteAsync(
                new CommandDefinition(
                    MarkProcessedSql,
                    new
                    {
                        message.Id,
                        ProcessedAt = deadLetteredAt,
                        Error = errorMessage,
                    },
                    cancellationToken: cancellationToken
                )
            )
            .ConfigureAwait(false);
    }
}
