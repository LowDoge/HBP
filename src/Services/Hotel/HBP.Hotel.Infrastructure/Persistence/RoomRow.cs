using HBP.Hotel.Domain;

namespace HBP.Hotel.Infrastructure.Persistence;

internal sealed class RoomRow
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = nameof(RoomStatus.Active);

    public static RoomRow From(HotelId hotelId, Room room) =>
        new()
        {
            Id = room.Id.Value,
            HotelId = hotelId.Value,
            Type = room.Type.ToString().ToLowerInvariant(),
            Capacity = room.Capacity,
            PricePerNight = room.PricePerNight.Amount,
            Currency = room.PricePerNight.Currency,
            Status = room.Status.ToString().ToLowerInvariant(),
        };

    public Room ToRoom()
    {
        return Room.Reconstitute(
            RoomId.From(Id),
            Enum.Parse<RoomType>(Type, ignoreCase: true),
            Capacity,
            new Money(PricePerNight, Currency),
            Enum.Parse<RoomStatus>(Status, ignoreCase: true)
        );
    }
}
