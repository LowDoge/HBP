using HBP.Common;
using HBP.Hotel.Application.Abstractions;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.GetHotel;

internal sealed class GetHotelQueryHandler(IHotelRepository repository, IHotelCache cache)
    : IRequestHandler<GetHotelQuery, Result<HotelDto>>
{
    public async Task<Result<HotelDto>> Handle(
        GetHotelQuery query,
        CancellationToken cancellationToken
    )
    {
        var cached = await cache
            .GetHotelAsync(query.HotelId, cancellationToken)
            .ConfigureAwait(false);

        if (cached is not null)
        {
            return Result<HotelDto>.Success(cached);
        }

        var hotel = await repository
            .GetAsync(query.HotelId, cancellationToken)
            .ConfigureAwait(false);

        if (hotel is null)
        {
            return Result<HotelDto>.Failure(
                Error.NotFound(nameof(Domain.Hotel), query.HotelId.Value)
            );
        }

        var dto = HotelDto.From(hotel);
        await cache.SetHotelAsync(dto, cancellationToken).ConfigureAwait(false);

        return Result<HotelDto>.Success(dto);
    }
}
