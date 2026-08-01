using HBP.Common;
using HBP.Hotel.Application.Abstractions;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.GetRooms;

internal sealed class GetRoomsQueryHandler(IHotelRepository repository)
    : IRequestHandler<GetRoomsQuery, Result<IReadOnlyList<RoomDto>>>
{
    public async Task<Result<IReadOnlyList<RoomDto>>> Handle(
        GetRoomsQuery query,
        CancellationToken cancellationToken
    )
    {
        var hotel = await repository
            .GetAsync(query.HotelId, cancellationToken)
            .ConfigureAwait(false);

        if (hotel is null)
        {
            return Result<IReadOnlyList<RoomDto>>.Failure(
                Error.NotFound(nameof(Domain.Hotel), query.HotelId.Value)
            );
        }

        return Result<IReadOnlyList<RoomDto>>.Success(hotel.Rooms.Select(RoomDto.From).ToList());
    }
}
