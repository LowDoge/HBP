using FluentAssertions;
using HBP.Hotel.Application.DomainEvents;
using HBP.Hotel.Domain;
using HBP.Hotel.Domain.Events;

namespace HBP.Hotel.Application.UnitTests.DomainEvents;

public class DomainEventNotificationTests
{
    [Fact]
    public void Create_WrapsRuntimeEventType()
    {
        var domainEvent = new HotelCreatedEvent(
            HotelId.New(),
            "Grand Hotel",
            "US",
            "New York",
            "5th Ave",
            null,
            DateTimeOffset.UtcNow
        );

        var notification = DomainEventNotification.Create(domainEvent);

        notification.Should().BeOfType<DomainEventNotification<HotelCreatedEvent>>();
        ((DomainEventNotification<HotelCreatedEvent>)notification).Event.Should().Be(domainEvent);
    }
}
