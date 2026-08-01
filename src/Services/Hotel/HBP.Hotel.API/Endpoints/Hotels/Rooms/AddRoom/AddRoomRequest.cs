using FastEndpoints;
using HBP.Hotel.Domain;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.AddRoom;

internal sealed class AddRoomRequest
{
    [BindFrom("id")]
    public Guid Id { get; set; }

    public RoomType Type { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public string Currency { get; set; } = string.Empty;
}
