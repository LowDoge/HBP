using HBP.Hotel.Application.IntegrationEvents;
using HBP.Hotel.Domain.Events;
using HBP.Messaging.Abstractions;
using MediatR;

namespace HBP.Hotel.Application.DomainEvents;

internal sealed class RoomStatusChangedEventNotificationHandler(IMessagePublisher publisher)
    : INotificationHandler<DomainEventNotification<RoomStatusChangedEvent>>
{
    public async Task Handle(
        DomainEventNotification<RoomStatusChangedEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var e = notification.Event;
        await publisher
            .PublishAsync(
                new RoomStatusChangedIntegrationEvent(
                    e.HotelId,
                    e.RoomId,
                    e.OldStatus,
                    e.NewStatus,
                    e.OccurredAt
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
