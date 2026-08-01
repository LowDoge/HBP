using HBP.Hotel.Application;

namespace HBP.Hotel.API.Endpoints;

internal sealed record AddressResponse(
    string Country,
    string City,
    string Street,
    string? PostalCode
)
{
    public static AddressResponse From(AddressDto dto) =>
        new(dto.Country, dto.City, dto.Street, dto.PostalCode);
}
