using HBP.Common;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.UpdateRoom;

public sealed record UpdateRoomCommand(
    HotelId HotelId,
    RoomId RoomId,
    int? Capacity,
    decimal? PricePerNight,
    string? Currency
) : IRequest<Result<RoomDto>>;
