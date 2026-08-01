using HBP.Hotel.Application.IntegrationEvents;
using HBP.Hotel.Domain.Events;
using HBP.Messaging.Abstractions;
using MediatR;

namespace HBP.Hotel.Application.DomainEvents;

internal sealed class RoomEditedEventNotificationHandler(IMessagePublisher publisher)
    : INotificationHandler<DomainEventNotification<RoomEditedEvent>>
{
    public async Task Handle(
        DomainEventNotification<RoomEditedEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var e = notification.Event;
        await publisher
            .PublishAsync(
                new RoomEditedIntegrationEvent(
                    e.HotelId,
                    e.RoomId,
                    e.Capacity,
                    e.PricePerNight,
                    e.Currency,
                    e.OccurredAt
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
