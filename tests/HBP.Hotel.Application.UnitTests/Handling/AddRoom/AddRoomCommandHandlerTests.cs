using System.Data;
using FluentAssertions;
using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Application.DomainEvents;
using HBP.Hotel.Application.Handlers.Hotels.AddRoom;
using HBP.Hotel.Domain;
using HBP.Hotel.Domain.Events;
using MediatR;
using Moq;

namespace HBP.Hotel.Application.UnitTests.Handling.AddRoom;

public class AddRoomCommandHandlerTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<IHotelRepository> _repository = new();

    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public AddRoomCommandHandlerTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(FixedNow);
        _unitOfWork
            .Setup(u => u.BeginAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        _unitOfWork
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        _unitOfWork
            .Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
    }

    private AddRoomCommandHandler CreateHandler() =>
        new(_unitOfWork.Object, _repository.Object, _clock.Object, _publisher.Object);

    private static HBP.Hotel.Domain.Hotel CreateHotel()
    {
        var hotel = HBP.Hotel.Domain.Hotel.Create(
            HotelId.New(),
            "Grand Hotel",
            new Address("US", "New York", "5th Ave"),
            FixedNow
        );
        return hotel;
    }

    [Fact]
    public async Task Handle_WhenHotelNotFound_ReturnsNotFound()
    {
        var handler = CreateHandler();
        var hotelId = HotelId.New();

        _repository
            .Setup(r => r.GetAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HBP.Hotel.Domain.Hotel?)null);

        var result = await handler.Handle(
            new AddRoomCommand(hotelId, RoomType.Single, 2, 100m, "USD"),
            CancellationToken.None
        );

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoomAdded_CommitsAndReturnsHotel()
    {
        var handler = CreateHandler();
        var hotel = CreateHotel();

        _repository
            .Setup(r => r.GetAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        var result = await handler.Handle(
            new AddRoomCommand(hotel.Id, RoomType.Double, 2, 150m, "USD"),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value!.Rooms.Should().ContainSingle(r => r.Type == RoomType.Double);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.UpdateAsync(hotel, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDuplicateRoom_ReturnsConflictAndRollsBack()
    {
        var handler = CreateHandler();
        var hotel = CreateHotel();
        hotel.AddRoom(RoomType.Single, 2, new Money(100m, "USD"), FixedNow);

        _repository
            .Setup(r => r.GetAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        var result = await handler.Handle(
            new AddRoomCommand(hotel.Id, RoomType.Single, 2, 120m, "USD"),
            CancellationToken.None
        );

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(
            r => r.UpdateAsync(It.IsAny<HBP.Hotel.Domain.Hotel>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenCapacityInvalid_ReturnsValidation()
    {
        var handler = CreateHandler();
        var hotel = CreateHotel();

        _repository
            .Setup(r => r.GetAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        var result = await handler.Handle(
            new AddRoomCommand(hotel.Id, RoomType.Single, 0, 100m, "USD"),
            CancellationToken.None
        );

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoomAdded_PublishesDomainEvent()
    {
        var handler = CreateHandler();
        var hotel = CreateHotel();

        _repository
            .Setup(r => r.GetAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        await handler.Handle(
            new AddRoomCommand(hotel.Id, RoomType.Double, 2, 150m, "USD"),
            CancellationToken.None
        );

        _publisher.Verify(
            p =>
                p.Publish(
                    It.Is<object>(o => o is DomainEventNotification<RoomAddedEvent>),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
