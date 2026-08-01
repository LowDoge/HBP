namespace HBP.Messaging.Kafka;

internal sealed class MessagingConfig
{
    public KafkaProducerConfig Kafka { get; set; } = null!;
    public OutboxProducerConfig Outbox { get; set; } = null!;
}
