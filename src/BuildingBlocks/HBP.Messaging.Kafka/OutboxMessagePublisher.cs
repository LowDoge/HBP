using System.Text.Json;
using HBP.Common;
using HBP.Messaging.Abstractions;

namespace HBP.Messaging.Kafka;

internal sealed class OutboxMessagePublisher(
    IClock clock,
    AddOnlyOutboxMessageRepository messageRepository
) : IMessagePublisher
{
    private readonly IClock _clock = Guard.AgainstNull(clock);

    private readonly AddOnlyOutboxMessageRepository _messageRepository = Guard.AgainstNull(
        messageRepository
    );

    public Task PublishAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        return PublishAsync(null, message, cancellationToken);
    }

    public async Task PublishAsync(
        string? key,
        IMessage message,
        CancellationToken cancellationToken = default
    )
    {
        Guard.AgainstNull(message);

        var topic = KafkaTopicResolver.Resolve(message);
        var payload = JsonSerializer.Serialize((object)message, JsonSerializerOptions.Web);
        var outboxMessage = new OutboxMessage(
            Guid.NewGuid(),
            topic,
            key,
            message.GetType().FullName ?? message.GetType().Name,
            payload,
            _clock.UtcNow.UtcDateTime
        );

        await _messageRepository.AddAsync(outboxMessage, cancellationToken);
    }
}
