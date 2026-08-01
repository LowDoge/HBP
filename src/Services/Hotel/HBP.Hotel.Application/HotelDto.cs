namespace HBP.Hotel.Application;

public sealed record HotelDto(
    Guid Id,
    string Name,
    AddressDto Address,
    IReadOnlyList<RoomDto> Rooms
)
{
    public static HotelDto From(Domain.Hotel hotel) =>
        new(
            hotel.Id.Value,
            hotel.Name,
            AddressDto.From(hotel.Address),
            hotel.Rooms.Select(RoomDto.From).ToList()
        );
}
