using FastEndpoints;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.Update;

internal sealed class UpdateRoomRequest
{
    [BindFrom("id")]
    public Guid Id { get; set; }

    [BindFrom("roomId")]
    public Guid RoomId { get; set; }

    public int? Capacity { get; set; }
    public decimal? PricePerNight { get; set; }
    public string? Currency { get; set; }
}
