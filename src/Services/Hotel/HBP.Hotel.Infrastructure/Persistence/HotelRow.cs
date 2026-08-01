namespace HBP.Hotel.Infrastructure.Persistence;

internal sealed class HotelRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? PostalCode { get; set; }

    public static HotelRow From(Domain.Hotel hotel) =>
        new()
        {
            Id = hotel.Id.Value,
            Name = hotel.Name,
            Country = hotel.Address.Country,
            City = hotel.Address.City,
            Street = hotel.Address.Street,
            PostalCode = hotel.Address.PostalCode,
        };
}
