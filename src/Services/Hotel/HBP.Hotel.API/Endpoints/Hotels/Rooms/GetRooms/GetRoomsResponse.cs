using HBP.Hotel.Application;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.GetRooms;

internal sealed record GetRoomsResponse(IReadOnlyList<RoomResponse> Rooms)
{
    public static GetRoomsResponse From(IEnumerable<RoomDto> rooms) =>
        new(rooms.Select(RoomResponse.From).ToList());
}
