using HBP.Hotel.Application.IntegrationEvents;
using HBP.Hotel.Domain.Events;
using HBP.Messaging.Abstractions;
using MediatR;

namespace HBP.Hotel.Application.DomainEvents;

internal sealed class HotelRenamedEventNotificationHandler(IMessagePublisher publisher)
    : INotificationHandler<DomainEventNotification<HotelRenamedEvent>>
{
    public async Task Handle(
        DomainEventNotification<HotelRenamedEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var e = notification.Event;
        await publisher
            .PublishAsync(
                new HotelRenamedIntegrationEvent(e.HotelId, e.NewName, e.OccurredAt),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
