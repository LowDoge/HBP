using HBP.Hotel.Domain;
using HBP.Messaging.Abstractions;

namespace HBP.Hotel.Application.IntegrationEvents;

[Topic("hotel.renamed")]
public sealed record HotelRenamedIntegrationEvent(
    HotelId HotelId,
    string NewName,
    DateTimeOffset OccurredAt
) : IMessage;
