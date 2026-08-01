using HBP.Common;
using Microsoft.Extensions.Logging;

namespace HBP.Messaging.Kafka;

internal sealed class OutboxBatchProcessor(
    OutboxProducerConfig config,
    IOutboxProducer producer,
    IClock clock,
    ILogger<OutboxBatchProcessor> logger
)
{
    private readonly OutboxProducerConfig _config = Guard.AgainstNull(config);
    private readonly IOutboxProducer _producer = Guard.AgainstNull(producer);
    private readonly IClock _clock = Guard.AgainstNull(clock);
    private readonly ILogger<OutboxBatchProcessor> _logger = Guard.AgainstNull(logger);

    public async Task ProcessBatchAsync(
        IOutboxMessageRepository repository,
        IReadOnlyList<OutboxMessage> messages,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _producer.ProduceAsync(message, cancellationToken).ConfigureAwait(false);
                await repository
                    .CompleteAsync(message.Id, _clock.UtcNow.UtcDateTime, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Message {Id} of type {Type} publishing failed",
                    message.Id,
                    message.Type
                );

                if (message.RetryCount + 1 >= _config.MaxRetries)
                {
                    _logger.LogWarning(
                        "Message {Id} of type {Type} exceeded max retries ({MaxRetries}), moving to DLQ",
                        message.Id,
                        message.Type,
                        _config.MaxRetries
                    );
                    await repository
                        .DeadLetterAsync(
                            message,
                            ex.ToString(),
                            _clock.UtcNow.UtcDateTime,
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                }
                else
                {
                    await repository
                        .FailAsync(message.Id, ex.ToString(), cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }
    }
}
