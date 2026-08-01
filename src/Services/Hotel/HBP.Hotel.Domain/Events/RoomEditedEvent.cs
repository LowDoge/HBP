using HBP.Common;

namespace HBP.Hotel.Domain.Events;

public sealed record RoomEditedEvent(
    HotelId HotelId,
    RoomId RoomId,
    int Capacity,
    decimal PricePerNight,
    string Currency,
    DateTimeOffset OccurredAt
) : DomainEvent(OccurredAt);
