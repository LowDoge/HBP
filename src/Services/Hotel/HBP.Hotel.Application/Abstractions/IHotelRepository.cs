using HBP.Hotel.Domain;

namespace HBP.Hotel.Application.Abstractions;

public interface IHotelRepository
{
    Task AddAsync(Domain.Hotel hotel, CancellationToken cancellationToken = default);

    Task UpdateAsync(Domain.Hotel hotel, CancellationToken cancellationToken = default);

    Task<Domain.Hotel?> GetAsync(HotelId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Hotel>> ListAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(HotelId id, CancellationToken cancellationToken = default);
}
