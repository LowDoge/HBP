using HBP.Hotel.Domain;
using HBP.Messaging.Abstractions;

namespace HBP.Hotel.Application.IntegrationEvents;

[Topic("room.status.changed")]
public sealed record RoomStatusChangedIntegrationEvent(
    HotelId HotelId,
    RoomId RoomId,
    RoomStatus OldStatus,
    RoomStatus NewStatus,
    DateTimeOffset OccurredAt
) : IMessage;
