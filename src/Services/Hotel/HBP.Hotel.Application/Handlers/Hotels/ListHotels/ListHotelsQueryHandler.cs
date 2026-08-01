using HBP.Hotel.Application.Abstractions;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.ListHotels;

internal sealed class ListHotelsQueryHandler(IHotelRepository repository, IHotelCache cache)
    : IRequestHandler<ListHotelsQuery, IReadOnlyList<HotelDto>>
{
    public async Task<IReadOnlyList<HotelDto>> Handle(
        ListHotelsQuery query,
        CancellationToken cancellationToken
    )
    {
        var skip = Math.Max(0, query.Skip);
        var take = Math.Clamp(query.Take, 1, 200);

        var cached = await cache
            .GetCatalogAsync(skip, take, cancellationToken)
            .ConfigureAwait(false);
        if (cached is not null)
        {
            return cached;
        }

        var hotels = await repository
            .ListAsync(skip, take, cancellationToken)
            .ConfigureAwait(false);
        var dtos = hotels.Select(HotelDto.From).ToList();
        await cache.SetCatalogAsync(dtos, skip, take, cancellationToken).ConfigureAwait(false);

        return dtos;
    }
}
