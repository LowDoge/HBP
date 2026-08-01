using HBP.Common;

namespace HBP.Hotel.Domain.Events;

public sealed record RoomAddedEvent(
    HotelId HotelId,
    RoomId RoomId,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    string Currency,
    DateTimeOffset OccurredAt
) : DomainEvent(OccurredAt);
