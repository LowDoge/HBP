using HBP.Hotel.Domain;
using HBP.Messaging.Abstractions;

namespace HBP.Hotel.Application.IntegrationEvents;

[Topic("room.added")]
public sealed record RoomAddedIntegrationEvent(
    HotelId HotelId,
    RoomId RoomId,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    string Currency,
    DateTimeOffset OccurredAt
) : IMessage;
