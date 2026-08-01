using FastEndpoints;
using HBP.Hotel.Domain;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.SetStatus;

internal sealed class SetRoomStatusRequest
{
    [BindFrom("id")]
    public Guid Id { get; set; }

    [BindFrom("roomId")]
    public Guid RoomId { get; set; }

    public RoomStatus Status { get; set; }
}
