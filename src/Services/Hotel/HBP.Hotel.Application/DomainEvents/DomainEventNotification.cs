using System.Collections.Concurrent;
using HBP.Common;
using MediatR;

namespace HBP.Hotel.Application.DomainEvents;

internal sealed record DomainEventNotification<TEvent>(TEvent Event) : INotification
    where TEvent : IDomainEvent;

internal static class DomainEventNotification
{
    private static readonly ConcurrentDictionary<Type, Type> WrapperTypes = new();

    public static INotification Create(IDomainEvent @event)
    {
        Guard.AgainstNull(@event);

        var wrapperType = WrapperTypes.GetOrAdd(
            @event.GetType(),
            static evType => typeof(DomainEventNotification<>).MakeGenericType(evType)
        );

        return (INotification)Activator.CreateInstance(wrapperType, @event)!;
    }
}
