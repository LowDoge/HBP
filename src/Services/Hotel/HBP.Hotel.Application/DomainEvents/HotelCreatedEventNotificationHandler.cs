using HBP.Hotel.Application.IntegrationEvents;
using HBP.Hotel.Domain.Events;
using HBP.Messaging.Abstractions;
using MediatR;

namespace HBP.Hotel.Application.DomainEvents;

internal sealed class HotelCreatedEventNotificationHandler(IMessagePublisher publisher)
    : INotificationHandler<DomainEventNotification<HotelCreatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<HotelCreatedEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var e = notification.Event;
        await publisher
            .PublishAsync(
                new HotelCreatedIntegrationEvent(
                    e.HotelId,
                    e.Name,
                    e.Country,
                    e.City,
                    e.Street,
                    e.PostalCode,
                    e.OccurredAt
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
