using HBP.Common;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.GetRooms;

public sealed record GetRoomsQuery(HotelId HotelId) : IRequest<Result<IReadOnlyList<RoomDto>>>;
