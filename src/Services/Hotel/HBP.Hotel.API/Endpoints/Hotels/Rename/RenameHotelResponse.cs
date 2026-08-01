using HBP.Hotel.Application;

namespace HBP.Hotel.API.Endpoints.Hotels.Rename;

internal sealed record RenameHotelResponse(
    Guid Id,
    string Name,
    AddressResponse Address,
    IReadOnlyList<RoomResponse> Rooms
)
{
    public static RenameHotelResponse From(HotelDto hotel) =>
        new(
            hotel.Id,
            hotel.Name,
            AddressResponse.From(hotel.Address),
            hotel.Rooms.Select(RoomResponse.From).ToList()
        );
}
