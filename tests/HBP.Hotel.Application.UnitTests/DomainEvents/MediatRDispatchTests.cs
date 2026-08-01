using HBP.Common;
using HBP.Hotel.Application.DomainEvents;
using HBP.Hotel.Application.IntegrationEvents;
using HBP.Hotel.Domain;
using HBP.Hotel.Domain.Events;
using HBP.Messaging.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HBP.Hotel.Application.UnitTests.DomainEvents;

public class MediatRDispatchTests
{
    [Fact]
    public async Task PublishObject_DispatchesToConcreteNotificationHandler()
    {
        var publisher = new Mock<IMessagePublisher>();
        var services = new ServiceCollection();
        services.AddSingleton(publisher.Object);
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(HotelCreatedEventNotificationHandler).Assembly)
        );
        await using var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IPublisher>();
        var domainEvent = new HotelCreatedEvent(
            HotelId.New(),
            "Grand Hotel",
            "US",
            "New York",
            "5th Ave",
            null,
            DateTimeOffset.UtcNow
        );

        await mediator.Publish((object)DomainEventNotification.Create(domainEvent));

        publisher.Verify(
            p =>
                p.PublishAsync(
                    It.Is<HotelCreatedIntegrationEvent>(e =>
                        e.HotelId == domainEvent.HotelId && e.Name == "Grand Hotel"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task PublishObject_WhenNoHandlerExists_DoesNothing()
    {
        var publisher = new Mock<IMessagePublisher>();
        var services = new ServiceCollection();
        services.AddSingleton(publisher.Object);
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(HotelCreatedEventNotificationHandler).Assembly)
        );
        await using var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IPublisher>();
        var unknownEvent = new UnhandledDomainEvent(DateTimeOffset.UtcNow);

        await mediator.Publish((object)DomainEventNotification.Create(unknownEvent));

        publisher.Verify(
            p => p.PublishAsync(It.IsAny<IMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    private sealed record UnhandledDomainEvent(DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
}
