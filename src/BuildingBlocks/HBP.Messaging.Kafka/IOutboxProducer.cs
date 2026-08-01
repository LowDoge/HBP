namespace HBP.Messaging.Kafka;

internal interface IOutboxProducer
{
    Task ProduceAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
