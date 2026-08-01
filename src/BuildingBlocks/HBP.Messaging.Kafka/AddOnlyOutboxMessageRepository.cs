using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Data.Postgres;

namespace HBP.Messaging.Kafka;

internal sealed class AddOnlyOutboxMessageRepository(IDbContext dbContext) : DbRepository(dbContext)
{
    private const string AddMessageSql = """
        INSERT INTO outbox_messages(id, topic, key, type, payload, created_at)
        VALUES (@Id, @Topic, @Key, @Type, @Payload::jsonb, @CreatedAt)
        """;

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        await ExecuteAsync(AddMessageSql, message, cancellationToken).ConfigureAwait(false);
    }
}
