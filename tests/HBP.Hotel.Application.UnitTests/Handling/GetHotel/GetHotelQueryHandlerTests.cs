using FluentAssertions;
using HBP.Common;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Application.Handlers.Hotels.GetHotel;
using HBP.Hotel.Domain;
using Moq;

namespace HBP.Hotel.Application.UnitTests.Handling.GetHotel;

public class GetHotelQueryHandlerTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
    private readonly Mock<IHotelCache> _cache = new();

    private readonly Mock<IHotelRepository> _repository = new();

    private GetHotelQueryHandler CreateHandler() => new(_repository.Object, _cache.Object);

    [Fact]
    public async Task Handle_WhenCached_ReturnsCachedHotelAndSkipsRepository()
    {
        var hotel = HBP.Hotel.Domain.Hotel.Create(
            HotelId.New(),
            "Grand Hotel",
            new Address("US", "New York", "5th Ave"),
            FixedNow
        );
        var cached = HotelDto.From(hotel);
        _cache
            .Setup(c => c.GetHotelAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await CreateHandler()
            .Handle(new GetHotelQuery(hotel.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cached);
        _repository.Verify(
            r => r.GetAsync(It.IsAny<HotelId>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _cache.Verify(
            c => c.SetHotelAsync(It.IsAny<HotelDto>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenCacheMissAndHotelFound_ReturnsHotelAndCaches()
    {
        var hotel = HBP.Hotel.Domain.Hotel.Create(
            HotelId.New(),
            "Grand Hotel",
            new Address("US", "New York", "5th Ave"),
            FixedNow
        );
        _cache
            .Setup(c => c.GetHotelAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HotelDto?)null);
        _repository
            .Setup(r => r.GetAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        var result = await CreateHandler()
            .Handle(new GetHotelQuery(hotel.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(hotel.Id.Value);
        _cache.Verify(
            c => c.SetHotelAsync(It.IsAny<HotelDto>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenCacheMissAndHotelNotFound_ReturnsNotFound()
    {
        var hotelId = HotelId.New();
        _cache
            .Setup(c => c.GetHotelAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HotelDto?)null);
        _repository
            .Setup(r => r.GetAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HBP.Hotel.Domain.Hotel?)null);

        var result = await CreateHandler()
            .Handle(new GetHotelQuery(hotelId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _cache.Verify(
            c => c.SetHotelAsync(It.IsAny<HotelDto>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
