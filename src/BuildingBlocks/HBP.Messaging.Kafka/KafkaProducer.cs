using Confluent.Kafka;
using HBP.Common;
using Microsoft.Extensions.Logging;

namespace HBP.Messaging.Kafka;

internal sealed class KafkaProducer : IOutboxProducer, IDisposable
{
    private readonly IClock _clock;
    private readonly IProducer<string, string> _producer;
    private bool _isDisposed;

    public KafkaProducer(KafkaProducerConfig config, IClock clock, ILogger<KafkaProducer> logger)
    {
        Guard.AgainstNull(config);
        Guard.AgainstNull(logger);
        _clock = Guard.AgainstNull(clock);

        var kafkaConfig = new ProducerConfig
        {
            ClientId = config.ClientId,
            BootstrapServers = config.BootstrapServers,
            MessageTimeoutMs = config.MessageTimeout,
            RequestTimeoutMs = config.RequestTimeout,
            Acks = Acks.Leader,
        };

        _producer = new ProducerBuilder<string, string>(kafkaConfig)
            .SetLogHandler(
                (_, message) =>
                {
                    var logLevel = message.Level.ToMsLogLevel();
                    logger.Log(logLevel, message.Message);
                }
            )
            .Build();
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _producer.Dispose();
        _isDisposed = true;

        GC.SuppressFinalize(this);
    }

    public async Task ProduceAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default
    )
    {
        Guard.AgainstNull(message);

        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(KafkaProducer));
        }

        var kafkaMessage = new Message<string, string>
        {
            Key = message.Key ?? null!,
            Value = message.Payload,
            Timestamp = new Timestamp(_clock.UtcNow),
        };

        await _producer
            .ProduceAsync(message.Topic, kafkaMessage, cancellationToken)
            .ConfigureAwait(false);
    }

    ~KafkaProducer()
    {
        Dispose();
    }
}
