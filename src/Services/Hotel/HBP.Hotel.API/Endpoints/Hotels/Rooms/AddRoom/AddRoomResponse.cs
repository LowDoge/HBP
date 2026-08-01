using HBP.Hotel.Application;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.AddRoom;

internal sealed record AddRoomResponse(
    Guid Id,
    string Name,
    AddressResponse Address,
    IReadOnlyList<RoomResponse> Rooms
)
{
    public static AddRoomResponse From(HotelDto hotel) =>
        new(
            hotel.Id,
            hotel.Name,
            AddressResponse.From(hotel.Address),
            hotel.Rooms.Select(RoomResponse.From).ToList()
        );
}
