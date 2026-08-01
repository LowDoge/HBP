using HBP.Hotel.Domain;

namespace HBP.Hotel.Application;

public sealed record AddressDto(string Country, string City, string Street, string? PostalCode)
{
    public static AddressDto From(Address address) =>
        new(address.Country, address.City, address.Street, address.PostalCode);
}
