using HBP.Common;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.GetHotel;

public sealed record GetHotelQuery(HotelId HotelId) : IRequest<Result<HotelDto>>;
