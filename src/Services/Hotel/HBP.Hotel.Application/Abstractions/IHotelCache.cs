using HBP.Hotel.Domain;

namespace HBP.Hotel.Application.Abstractions;

public interface IHotelCache
{
    Task<HotelDto?> GetHotelAsync(HotelId id, CancellationToken cancellationToken = default);

    Task SetHotelAsync(HotelDto hotel, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HotelDto>?> GetCatalogAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    Task SetCatalogAsync(
        IReadOnlyList<HotelDto> hotels,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    Task InvalidateAsync(HotelId hotelId, CancellationToken cancellationToken = default);
}
