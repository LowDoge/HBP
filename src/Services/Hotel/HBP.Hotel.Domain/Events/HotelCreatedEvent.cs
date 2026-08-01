using HBP.Common;

namespace HBP.Hotel.Domain.Events;

public sealed record HotelCreatedEvent(
    HotelId HotelId,
    string Name,
    string Country,
    string City,
    string Street,
    string? PostalCode,
    DateTimeOffset OccurredAt
) : DomainEvent(OccurredAt);
