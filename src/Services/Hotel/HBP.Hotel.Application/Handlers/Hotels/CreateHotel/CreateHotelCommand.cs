using HBP.Common;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.CreateHotel;

public sealed record CreateHotelCommand(
    string Name,
    string Country,
    string City,
    string Street,
    string? PostalCode
) : IRequest<Result<HotelDto>>;
