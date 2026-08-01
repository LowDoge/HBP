namespace HBP.Hotel.API.Endpoints.Hotels.Create;

internal sealed record CreateHotelRequest(
    string Name,
    string Country,
    string City,
    string Street,
    string? PostalCode
);
