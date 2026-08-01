using FluentAssertions;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Application.Handlers.Hotels.ListHotels;
using HBP.Hotel.Domain;
using Moq;

namespace HBP.Hotel.Application.UnitTests.Handling.ListHotels;

public class ListHotelsQueryHandlerTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
    private readonly Mock<IHotelCache> _cache = new();

    private readonly Mock<IHotelRepository> _repository = new();

    private ListHotelsQueryHandler CreateHandler() => new(_repository.Object, _cache.Object);

    private static HBP.Hotel.Domain.Hotel CreateHotel() =>
        HBP.Hotel.Domain.Hotel.Create(
            HotelId.New(),
            "Grand Hotel",
            new Address("US", "New York", "5th Ave"),
            FixedNow
        );

    [Fact]
    public async Task Handle_WhenCached_ReturnsCachedHotelsAndSkipsRepository()
    {
        var cached = new List<HotelDto> { HotelDto.From(CreateHotel()) };
        _cache
            .Setup(c => c.GetCatalogAsync(0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await CreateHandler()
            .Handle(new ListHotelsQuery(0, 50), CancellationToken.None);

        result.Should().BeEquivalentTo(cached);
        _repository.Verify(
            r => r.ListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _cache.Verify(
            c =>
                c.SetCatalogAsync(
                    It.IsAny<IReadOnlyList<HotelDto>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ReturnsHotelsAndCaches()
    {
        var hotels = new List<HBP.Hotel.Domain.Hotel> { CreateHotel() };
        _cache
            .Setup(c =>
                c.GetCatalogAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((IReadOnlyList<HotelDto>?)null);
        _repository
            .Setup(r =>
                r.ListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(hotels);

        var result = await CreateHandler()
            .Handle(new ListHotelsQuery(0, 50), CancellationToken.None);

        result.Should().HaveCount(1);
        _cache.Verify(
            c =>
                c.SetCatalogAsync(
                    It.IsAny<IReadOnlyList<HotelDto>>(),
                    0,
                    50,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
