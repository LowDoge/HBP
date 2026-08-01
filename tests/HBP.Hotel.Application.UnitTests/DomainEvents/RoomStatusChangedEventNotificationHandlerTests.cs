using HBP.Hotel.Application.DomainEvents;
using HBP.Hotel.Application.IntegrationEvents;
using HBP.Hotel.Domain;
using HBP.Hotel.Domain.Events;
using HBP.Messaging.Abstractions;
using Moq;

namespace HBP.Hotel.Application.UnitTests.DomainEvents;

public class RoomStatusChangedEventNotificationHandlerTests
{
    [Fact]
    public async Task Handle_PublishesMappedIntegrationEvent()
    {
        var publisher = new Mock<IMessagePublisher>();
        var handler = new RoomStatusChangedEventNotificationHandler(publisher.Object);
        var occurredAt = DateTimeOffset.UtcNow;
        var domainEvent = new RoomStatusChangedEvent(
            HotelId.New(),
            RoomId.New(),
            RoomStatus.Active,
            RoomStatus.Maintenance,
            occurredAt
        );

        await handler.Handle(
            new DomainEventNotification<RoomStatusChangedEvent>(domainEvent),
            CancellationToken.None
        );

        publisher.Verify(
            p =>
                p.PublishAsync(
                    It.Is<RoomStatusChangedIntegrationEvent>(e =>
                        e.HotelId == domainEvent.HotelId
                        && e.RoomId == domainEvent.RoomId
                        && e.OldStatus == RoomStatus.Active
                        && e.NewStatus == RoomStatus.Maintenance
                        && e.OccurredAt == occurredAt
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
