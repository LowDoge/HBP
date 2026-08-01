using HBP.Common;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.SetRoomStatus;

public sealed record SetRoomStatusCommand(HotelId HotelId, RoomId RoomId, RoomStatus Status)
    : IRequest<Result<RoomDto>>;
