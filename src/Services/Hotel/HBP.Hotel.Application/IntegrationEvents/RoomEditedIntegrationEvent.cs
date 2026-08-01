using HBP.Hotel.Domain;
using HBP.Messaging.Abstractions;

namespace HBP.Hotel.Application.IntegrationEvents;

[Topic("room.edited")]
public sealed record RoomEditedIntegrationEvent(
    HotelId HotelId,
    RoomId RoomId,
    int Capacity,
    decimal PricePerNight,
    string Currency,
    DateTimeOffset OccurredAt
) : IMessage;
