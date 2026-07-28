using System.Text.Json;
using Dapper;
using HBP.Common;
using HBP.Persistence.Abstractions;
using HBP.Persistence.Outbox;

namespace HBP.Persistence.Postgres;

internal sealed class PostgresOutboxStore(IUnitOfWork unitOfWork) : IOutboxStore
{
    private const string InsertSql = """
                                     INSERT INTO outbox_messages
                                        (id, occurred_at, event_type, payload, processed_at, retry_count, last_error)
                                     VALUES
                                         (@Id, @OccurredAt, @EventType, @Payload::jsonb, @ProcessedAt, @RetryCount, @LastError);
                                     """;


    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IUnitOfWork _unitOfWork = Guard.AgainstNull(unitOfWork, nameof(unitOfWork));

    public Task AddAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!_unitOfWork.IsActive)
        {
            throw new
                InvalidOperationException("Outbox requires an active unit of work. Begin transaction before adding domain events");
        }

        var payload = JsonSerializer.Serialize(@event, JsonOptions);
        var message = OutboxMessage.Create(@event, payload);

        return _unitOfWork.Connection!.ExecuteAsync(new CommandDefinition(InsertSql, message, _unitOfWork.Transaction,
                                                                         cancellationToken: cancellationToken));
    }
}
