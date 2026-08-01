using HBP.Common;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.AddRoom;

public sealed record AddRoomCommand(
    HotelId HotelId,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    string Currency
) : IRequest<Result<HotelDto>>;
