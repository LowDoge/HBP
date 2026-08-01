using System.Text.Json;
using HBP.Common;
using HBP.Hotel.Application;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Domain;
using Microsoft.Extensions.Caching.Distributed;

namespace HBP.Hotel.Infrastructure.Caching;

internal sealed class HotelCache(IDistributedCache cache) : IHotelCache
{
    private const string VersionKey = "hotels:version";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);
    private readonly IDistributedCache _cache = Guard.AgainstNull(cache);

    public async Task<HotelDto?> GetHotelAsync(
        HotelId id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var json = await _cache.GetStringAsync($"hotel:{id.Value}", cancellationToken);
            return json is null
                ? null
                : JsonSerializer.Deserialize<HotelDto>(json, JsonSerializerOptions.Web);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return null;
        }
    }

    public async Task SetHotelAsync(HotelDto hotel, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(hotel, JsonSerializerOptions.Web);
            await _cache.SetStringAsync(
                $"hotel:{hotel.Id}",
                json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl },
                cancellationToken
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { }
    }

    public async Task<IReadOnlyList<HotelDto>?> GetCatalogAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var version = await _cache.GetStringAsync(VersionKey, cancellationToken) ?? "0";
            var json = await _cache.GetStringAsync(
                $"hotels:list:{version}:{skip}:{take}",
                cancellationToken
            );
            return json is null
                ? null
                : JsonSerializer.Deserialize<List<HotelDto>>(json, JsonSerializerOptions.Web);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return null;
        }
    }

    public async Task SetCatalogAsync(
        IReadOnlyList<HotelDto> hotels,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var version = await _cache.GetStringAsync(VersionKey, cancellationToken) ?? "0";
            var json = JsonSerializer.Serialize(hotels, JsonSerializerOptions.Web);
            await _cache.SetStringAsync(
                $"hotels:list:{version}:{skip}:{take}",
                json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl },
                cancellationToken
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { }
    }

    public async Task InvalidateAsync(
        HotelId hotelId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _cache.RemoveAsync($"hotel:{hotelId.Value}", cancellationToken);

            var current = await _cache.GetStringAsync(VersionKey, cancellationToken);
            var next = int.TryParse(current, out var version) ? version + 1 : 1;
            await _cache.SetStringAsync(VersionKey, next.ToString(), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { }
    }
}
