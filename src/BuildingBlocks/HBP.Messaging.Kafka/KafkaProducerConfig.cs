namespace HBP.Messaging.Kafka;

internal sealed class KafkaProducerConfig
{
    public string ClientId { get; set; } = null!;
    public string BootstrapServers { get; set; } = null!;
    public int MessageTimeout { get; set; } = 5000;
    public int RequestTimeout { get; set; } = 2000;
}
