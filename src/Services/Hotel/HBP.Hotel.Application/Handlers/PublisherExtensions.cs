using HBP.Common;
using HBP.Hotel.Application.DomainEvents;
using MediatR;

namespace HBP.Hotel.Application.Handlers;

internal static class PublisherExtensions
{
    public static async Task PublishDomainEventAsync(
        this IPublisher publisher,
        IDomainEvent @event,
        CancellationToken cancellationToken = default
    )
    {
        await publisher
            .Publish((object)DomainEventNotification.Create(@event), cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task PublishDomainEventsAsync(
        this IPublisher publisher,
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var ev in events)
        {
            await publisher.PublishDomainEventAsync(ev, cancellationToken).ConfigureAwait(false);
        }
    }

    public static async Task PublishDomainEventsAsync<TAggregateId>(
        this IPublisher publisher,
        AggregateRoot<TAggregateId> aggregate,
        bool clearEvents = true,
        CancellationToken cancellationToken = default
    )
        where TAggregateId : notnull
    {
        await publisher.PublishDomainEventsAsync(aggregate.DomainEvents, cancellationToken);
        if (clearEvents)
        {
            aggregate.ClearDomainEvents();
        }
    }
}
