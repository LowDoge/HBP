using HBP.Hotel.Application;
using HBP.Hotel.Domain;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.Update;

internal sealed record UpdateRoomResponse(
    Guid Id,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    string Currency,
    RoomStatus Status
)
{
    public static UpdateRoomResponse From(RoomDto room) =>
        new(room.Id, room.Type, room.Capacity, room.PricePerNight, room.Currency, room.Status);
}
