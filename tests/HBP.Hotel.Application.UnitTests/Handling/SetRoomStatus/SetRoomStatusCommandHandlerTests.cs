using System.Data;
using FluentAssertions;
using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Application.DomainEvents;
using HBP.Hotel.Application.Handlers.Hotels.SetRoomStatus;
using HBP.Hotel.Domain;
using HBP.Hotel.Domain.Events;
using MediatR;
using Moq;

namespace HBP.Hotel.Application.UnitTests.Handling.SetRoomStatus;

public class SetRoomStatusCommandHandlerTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<IHotelRepository> _repository = new();

    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public SetRoomStatusCommandHandlerTests()
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

    private SetRoomStatusCommandHandler CreateHandler() =>
        new(_unitOfWork.Object, _repository.Object, _clock.Object, _publisher.Object);

    private static HBP.Hotel.Domain.Hotel CreateHotelWithRoom(out Room room)
    {
        var hotel = HBP.Hotel.Domain.Hotel.Create(
            HotelId.New(),
            "Grand Hotel",
            new Address("US", "New York", "5th Ave"),
            FixedNow
        );
        room = hotel.AddRoom(RoomType.Single, 2, new Money(100m, "USD"), FixedNow);
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
            new SetRoomStatusCommand(hotelId, RoomId.New(), RoomStatus.Maintenance),
            CancellationToken.None
        );

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoomNotFound_ReturnsNotFound()
    {
        var handler = CreateHandler();
        var hotel = CreateHotelWithRoom(out _);
        var unknownRoomId = RoomId.New();

        _repository
            .Setup(r => r.GetAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        var result = await handler.Handle(
            new SetRoomStatusCommand(hotel.Id, unknownRoomId, RoomStatus.Maintenance),
            CancellationToken.None
        );

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenStatusUnchanged_ReturnsCurrentRoom()
    {
        var handler = CreateHandler();
        var hotel = CreateHotelWithRoom(out var room);

        _repository
            .Setup(r => r.GetAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        var result = await handler.Handle(
            new SetRoomStatusCommand(hotel.Id, room.Id, RoomStatus.Active),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(RoomStatus.Active);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.UpdateAsync(hotel, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStatusChanged_CommitsAndReturnsUpdatedRoom()
    {
        var handler = CreateHandler();
        var hotel = CreateHotelWithRoom(out var room);

        _repository
            .Setup(r => r.GetAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        var result = await handler.Handle(
            new SetRoomStatusCommand(hotel.Id, room.Id, RoomStatus.Maintenance),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(RoomStatus.Maintenance);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.UpdateAsync(hotel, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStatusChanged_PublishesDomainEvent()
    {
        var handler = CreateHandler();
        var hotel = CreateHotelWithRoom(out var room);

        _repository
            .Setup(r => r.GetAsync(hotel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        await handler.Handle(
            new SetRoomStatusCommand(hotel.Id, room.Id, RoomStatus.Maintenance),
            CancellationToken.None
        );

        _publisher.Verify(
            p =>
                p.Publish(
                    It.Is<object>(o => o is DomainEventNotification<RoomStatusChangedEvent>),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
