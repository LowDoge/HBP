using HBP.Hotel.Domain;
using HBP.Messaging.Abstractions;

namespace HBP.Hotel.Application.IntegrationEvents;

[Topic("hotel.created")]
public sealed record HotelCreatedIntegrationEvent(
    HotelId HotelId,
    string Name,
    string Country,
    string City,
    string Street,
    string? PostalCode,
    DateTimeOffset OccurredAt
) : IMessage;
