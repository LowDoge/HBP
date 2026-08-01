namespace HBP.Hotel.Domain;

public readonly record struct HotelId(Guid Value)
{
    public static HotelId New() => new(Guid.NewGuid());

    public static HotelId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
