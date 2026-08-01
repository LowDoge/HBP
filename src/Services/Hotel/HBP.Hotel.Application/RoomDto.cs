using HBP.Hotel.Domain;

namespace HBP.Hotel.Application;

public sealed record RoomDto(
    Guid Id,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    string Currency,
    RoomStatus Status
)
{
    public static RoomDto From(Room room) =>
        new(
            room.Id.Value,
            room.Type,
            room.Capacity,
            room.PricePerNight.Amount,
            room.PricePerNight.Currency,
            room.Status
        );
}
