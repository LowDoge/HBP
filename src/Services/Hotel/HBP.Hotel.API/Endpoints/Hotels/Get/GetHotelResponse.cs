using HBP.Hotel.Application;

namespace HBP.Hotel.API.Endpoints.Hotels.Get;

internal sealed record GetHotelResponse(
    Guid Id,
    string Name,
    AddressResponse Address,
    IReadOnlyList<RoomResponse> Rooms
)
{
    public static GetHotelResponse From(HotelDto hotel) =>
        new(
            hotel.Id,
            hotel.Name,
            AddressResponse.From(hotel.Address),
            hotel.Rooms.Select(RoomResponse.From).ToList()
        );
}
