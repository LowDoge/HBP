namespace HBP.Messaging.Abstractions;

public interface IMessagePublisher
{
    public Task PublishAsync(IMessage message, CancellationToken cancellationToken = default);

    public Task PublishAsync(
        string key,
        IMessage message,
        CancellationToken cancellationToken = default
    );
}
