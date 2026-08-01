using HBP.Hotel.Application;

namespace HBP.Hotel.API.Endpoints.Hotels.Create;

internal sealed record CreateHotelResponse(
    Guid Id,
    string Name,
    AddressResponse Address,
    IReadOnlyList<RoomResponse> Rooms
)
{
    public static CreateHotelResponse From(HotelDto hotel) =>
        new(
            hotel.Id,
            hotel.Name,
            AddressResponse.From(hotel.Address),
            hotel.Rooms.Select(RoomResponse.From).ToList()
        );
}
