using System.Text;
using System.Text.Json;
using FluentAssertions;
using HBP.Hotel.Application;
using HBP.Hotel.Domain;
using HBP.Hotel.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace HBP.Hotel.Infrastructure.UnitTests.Caching;

public class HotelCacheTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web
    );

    private readonly Mock<IDistributedCache> _cache = new();

    private HotelCache CreateCache() => new(_cache.Object);

    private static HotelDto CreateHotel(Guid? id = null) =>
        new(
            id ?? Guid.NewGuid(),
            "Grand Hotel",
            new AddressDto("US", "New York", "5th Ave", "10001"),
            new List<RoomDto>
            {
                new(Guid.NewGuid(), RoomType.Single, 2, 100m, "USD", RoomStatus.Active),
            }
        );

    private static byte[] Serialize(HotelDto hotel) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(hotel, SerializerOptions));

    private static byte[] Serialize(IReadOnlyList<HotelDto> hotels) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(hotels, SerializerOptions));

    [Fact]
    public async Task GetHotelAsync_WhenCached_ReturnsHotel()
    {
        var hotel = CreateHotel();
        _cache
            .Setup(c => c.GetAsync($"hotel:{hotel.Id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Serialize(hotel));

        var result = await CreateCache().GetHotelAsync(HotelId.From(hotel.Id));

        result.Should().BeEquivalentTo(hotel);
    }

    [Fact]
    public async Task GetHotelAsync_WhenMissing_ReturnsNull()
    {
        _cache
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await CreateCache().GetHotelAsync(HotelId.New());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetHotelAsync_WhenCacheFails_ReturnsNull()
    {
        _cache
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("redis down"));

        var result = await CreateCache().GetHotelAsync(HotelId.New());

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetHotelAsync_WritesJsonWithOneHourTtl()
    {
        var hotel = CreateHotel();
        var cache = CreateCache();

        await cache.SetHotelAsync(hotel);

        _cache.Verify(
            c =>
                c.SetAsync(
                    $"hotel:{hotel.Id}",
                    Serialize(hotel),
                    It.Is<DistributedCacheEntryOptions>(o =>
                        o.AbsoluteExpirationRelativeToNow == TimeSpan.FromHours(1)
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetCatalogAsync_WhenCached_ReturnsHotels()
    {
        var hotels = new List<HotelDto> { CreateHotel() };
        _cache
            .Setup(c => c.GetAsync("hotels:version", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes("3"));
        _cache
            .Setup(c => c.GetAsync("hotels:list:3:0:50", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Serialize(hotels));

        var result = await CreateCache().GetCatalogAsync(0, 50);

        result.Should().BeEquivalentTo(hotels);
    }

    [Fact]
    public async Task GetCatalogAsync_WhenMissing_ReturnsNull()
    {
        _cache
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await CreateCache().GetCatalogAsync(0, 50);

        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateAsync_RemovesHotelAndBumpsVersion()
    {
        var hotelId = Guid.NewGuid();
        _cache
            .Setup(c => c.GetAsync("hotels:version", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes("3"));

        await CreateCache().InvalidateAsync(HotelId.From(hotelId));

        _cache.Verify(
            c => c.RemoveAsync($"hotel:{hotelId}", It.IsAny<CancellationToken>()),
            Times.Once
        );
        _cache.Verify(
            c =>
                c.SetAsync(
                    "hotels:version",
                    Encoding.UTF8.GetBytes("4"),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task InvalidateAsync_WhenVersionMissing_SetsOne()
    {
        _cache
            .Setup(c => c.GetAsync("hotels:version", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        await CreateCache().InvalidateAsync(HotelId.New());

        _cache.Verify(
            c =>
                c.SetAsync(
                    "hotels:version",
                    Encoding.UTF8.GetBytes("1"),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task InvalidateAsync_WhenCacheFails_DoesNotThrow()
    {
        _cache
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("redis down"));

        var act = () => CreateCache().InvalidateAsync(HotelId.New());

        await act.Should().NotThrowAsync();
    }
}
