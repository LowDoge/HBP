using HBP.Common;

namespace HBP.Hotel.Domain;

public sealed class Room : Entity<RoomId>
{
    internal Room(RoomId id, RoomType type, int capacity, Money pricePerNight)
        : base(id)
    {
        Id = id;
        Type = type;
        Capacity = capacity;
        PricePerNight = pricePerNight;
    }

    public RoomType Type { get; private set; }
    public int Capacity { get; private set; }
    public Money PricePerNight { get; private set; }
    public RoomStatus Status { get; private set; } = RoomStatus.Active;

    internal void ChangePrice(Money newPrice) => PricePerNight = newPrice;

    internal void ChangeCapacity(int newCapacity)
    {
        if (newCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(newCapacity),
                "Capacity must be positive."
            );
        }

        Capacity = newCapacity;
    }

    internal void ChangeStatus(RoomStatus newStatus) => Status = newStatus;

    public static Room Reconstitute(
        RoomId id,
        RoomType type,
        int capacity,
        Money pricePerNight,
        RoomStatus status
    )
    {
        return new Room(id, type, capacity, pricePerNight) { Status = status };
    }
}
