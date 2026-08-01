namespace HBP.Messaging.Kafka;

internal sealed class OutboxProducerConfig
{
    public int BatchSize { get; set; } = 100;
    public int PollIntervalSeconds { get; set; } = 5;
    public int MaxRetries { get; set; } = 5;
}
