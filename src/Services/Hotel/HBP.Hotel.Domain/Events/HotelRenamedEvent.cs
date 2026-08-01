using HBP.Common;

namespace HBP.Hotel.Domain.Events;

public sealed record HotelRenamedEvent(HotelId HotelId, string NewName, DateTimeOffset OccurredAt)
    : DomainEvent(OccurredAt);
