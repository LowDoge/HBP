using HBP.Hotel.Application;
using HBP.Hotel.Domain;

namespace HBP.Hotel.API.Endpoints;

internal sealed record RoomResponse(
    Guid Id,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    string Currency,
    RoomStatus Status
)
{
    public static RoomResponse From(RoomDto dto) =>
        new(dto.Id, dto.Type, dto.Capacity, dto.PricePerNight, dto.Currency, dto.Status);
}
