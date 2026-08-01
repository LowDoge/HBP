using HBP.Common;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.DeleteHotel;

public sealed record DeleteHotelCommand(HotelId HotelId) : IRequest<Result>;
