using HBP.Common;
using HBP.Hotel.Domain.Events;

namespace HBP.Hotel.Domain;

public sealed class Hotel : AggregateRoot<HotelId>
{
    private readonly List<Room> _rooms = new();

    private Hotel(HotelId id, string name, Address address)
        : base(id)
    {
        Id = id;
        Name = name;
        Address = address;
    }

    public string Name { get; private set; }
    public Address Address { get; private set; }
    public IReadOnlyList<Room> Rooms => _rooms;

    public static Hotel Create(HotelId id, string name, Address address, DateTimeOffset occurredAt)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstNull(address, nameof(address));

        var hotel = new Hotel(id, name, address);
        hotel.AddDomainEvent(
            new HotelCreatedEvent(
                id,
                name,
                address.Country,
                address.City,
                address.Street,
                address.PostalCode,
                occurredAt
            )
        );

        return hotel;
    }

    public void Rename(string newName, DateTimeOffset occurredAt)
    {
        Guard.AgainstNullOrWhiteSpace(newName, nameof(newName));

        if (Name == newName)
        {
            return;
        }

        Name = newName;
        AddDomainEvent(new HotelRenamedEvent(Id, newName, occurredAt));
    }

    public Room AddRoom(RoomType type, int capacity, Money pricePerNight, DateTimeOffset occurredAt)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
        }

        if (_rooms.Any(r => r.Type == type && r.Capacity == capacity))
        {
            throw new InvalidOperationException(
                $"Room with type '{type}' and capacity {capacity} already exists."
            );
        }

        var roomId = RoomId.New();
        var room = new Room(roomId, type, capacity, pricePerNight);
        _rooms.Add(room);

        AddDomainEvent(
            new RoomAddedEvent(
                Id,
                roomId,
                type,
                capacity,
                pricePerNight.Amount,
                pricePerNight.Currency,
                occurredAt
            )
        );

        return room;
    }

    public void SetRoomStatus(RoomId roomId, RoomStatus newStatus, DateTimeOffset occurredAt)
    {
        var room =
            _rooms.FirstOrDefault(r => r.Id.Equals(roomId))
            ?? throw new ArgumentException($"Room '{roomId}' not found.", nameof(roomId));

        var oldStatus = room.Status;
        if (oldStatus == newStatus)
        {
            return;
        }

        room.ChangeStatus(newStatus);
        AddDomainEvent(new RoomStatusChangedEvent(Id, roomId, oldStatus, newStatus, occurredAt));
    }

    public void UpdateRoom(
        RoomId roomId,
        Money? pricePerNight,
        int? capacity,
        DateTimeOffset occurredAt
    )
    {
        var room =
            _rooms.FirstOrDefault(r => r.Id.Equals(roomId))
            ?? throw new ArgumentException($"Room '{roomId}' not found.", nameof(roomId));

        var changed = false;
        if (capacity is not null)
        {
            room.ChangeCapacity(capacity.Value);
            changed = true;
        }

        if (pricePerNight is not null)
        {
            room.ChangePrice(pricePerNight);
            changed = true;
        }

        if (changed)
        {
            AddDomainEvent(
                new RoomEditedEvent(
                    Id,
                    roomId,
                    room.Capacity,
                    room.PricePerNight.Amount,
                    room.PricePerNight.Currency,
                    occurredAt
                )
            );
        }
    }

    public static Hotel Reconstitute(
        HotelId id,
        string name,
        Address address,
        IEnumerable<Room> rooms
    )
    {
        var hotel = new Hotel(id, name, address);
        foreach (var room in rooms)
        {
            hotel._rooms.Add(room);
        }

        return hotel;
    }
}
