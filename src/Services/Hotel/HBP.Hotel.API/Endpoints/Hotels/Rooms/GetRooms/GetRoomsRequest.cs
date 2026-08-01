using FastEndpoints;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.GetRooms;

internal sealed class GetRoomsRequest
{
    [BindFrom("id")]
    public Guid Id { get; set; }
}
