using HBP.Hotel.Application.DomainEvents;
using HBP.Hotel.Application.IntegrationEvents;
using HBP.Hotel.Domain;
using HBP.Hotel.Domain.Events;
using HBP.Messaging.Abstractions;
using Moq;

namespace HBP.Hotel.Application.UnitTests.DomainEvents;

public class HotelCreatedEventNotificationHandlerTests
{
    [Fact]
    public async Task Handle_PublishesMappedIntegrationEvent()
    {
        var publisher = new Mock<IMessagePublisher>();
        var handler = new HotelCreatedEventNotificationHandler(publisher.Object);
        var occurredAt = DateTimeOffset.UtcNow;
        var domainEvent = new HotelCreatedEvent(
            HotelId.New(),
            "Grand Hotel",
            "US",
            "New York",
            "5th Ave",
            "10001",
            occurredAt
        );

        await handler.Handle(
            new DomainEventNotification<HotelCreatedEvent>(domainEvent),
            CancellationToken.None
        );

        publisher.Verify(
            p =>
                p.PublishAsync(
                    It.Is<HotelCreatedIntegrationEvent>(e =>
                        e.HotelId == domainEvent.HotelId
                        && e.Name == "Grand Hotel"
                        && e.Country == "US"
                        && e.City == "New York"
                        && e.Street == "5th Ave"
                        && e.PostalCode == "10001"
                        && e.OccurredAt == occurredAt
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
