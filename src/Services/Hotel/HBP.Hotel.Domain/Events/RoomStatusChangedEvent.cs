using HBP.Common;

namespace HBP.Hotel.Domain.Events;

public sealed record RoomStatusChangedEvent(
    HotelId HotelId,
    RoomId RoomId,
    RoomStatus OldStatus,
    RoomStatus NewStatus,
    DateTimeOffset OccurredAt
) : DomainEvent(OccurredAt);
