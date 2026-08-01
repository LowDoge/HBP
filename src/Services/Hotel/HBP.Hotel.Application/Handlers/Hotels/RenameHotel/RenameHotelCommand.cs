using HBP.Common;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.RenameHotel;

public sealed record RenameHotelCommand(HotelId HotelId, string NewName)
    : IRequest<Result<HotelDto>>;
