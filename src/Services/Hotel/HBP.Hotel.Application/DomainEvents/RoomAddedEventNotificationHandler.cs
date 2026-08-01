using HBP.Hotel.Application.IntegrationEvents;
using HBP.Hotel.Domain.Events;
using HBP.Messaging.Abstractions;
using MediatR;

namespace HBP.Hotel.Application.DomainEvents;

internal sealed class RoomAddedEventNotificationHandler(IMessagePublisher publisher)
    : INotificationHandler<DomainEventNotification<RoomAddedEvent>>
{
    public async Task Handle(
        DomainEventNotification<RoomAddedEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var e = notification.Event;
        await publisher
            .PublishAsync(
                new RoomAddedIntegrationEvent(
                    e.HotelId,
                    e.RoomId,
                    e.Type,
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
