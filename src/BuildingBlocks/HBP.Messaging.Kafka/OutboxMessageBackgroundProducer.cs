using HBP.Common;
using HBP.Data.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HBP.Messaging.Kafka;

internal sealed class OutboxMessageBackgroundProducer(
    OutboxProducerConfig config,
    IDbConnectionFactory connectionFactory,
    OutboxBatchProcessor batchProcessor,
    ILogger<OutboxMessageBackgroundProducer> logger
) : BackgroundService
{
    private readonly OutboxBatchProcessor _batchProcessor = Guard.AgainstNull(batchProcessor);
    private readonly OutboxProducerConfig _config = Guard.AgainstNull(config);
    private readonly IDbConnectionFactory _connectionFactory = Guard.AgainstNull(connectionFactory);
    private readonly ILogger<OutboxMessageBackgroundProducer> _logger = Guard.AgainstNull(logger);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Outbox producer started. Batch size {BatchSize}, poll interval {PollInterval}",
            _config.BatchSize,
            _config.PollIntervalSeconds
        );

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(cancellationToken).ConfigureAwait(false);
                await Task.Delay(
                        TimeSpan.FromSeconds(_config.PollIntervalSeconds),
                        cancellationToken
                    )
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox producer loop failed");
                await Task.Delay(
                        TimeSpan.FromSeconds(_config.PollIntervalSeconds),
                        cancellationToken
                    )
                    .ConfigureAwait(false);
            }
        }

        _logger.LogInformation("Outbox producer stopped");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.Open();
        var messagesRepository = new ProcessingOutboxMessageRepository(connection);

        var messages = await messagesRepository
            .ListUnprocessedAsync(_config.BatchSize, cancellationToken)
            .ConfigureAwait(false);

        if (messages.Count == 0)
        {
            return;
        }

        await _batchProcessor
            .ProcessBatchAsync(messagesRepository, messages, cancellationToken)
            .ConfigureAwait(false);
    }
}
