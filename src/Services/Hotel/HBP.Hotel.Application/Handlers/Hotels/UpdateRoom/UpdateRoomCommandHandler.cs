using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.UpdateRoom;

internal sealed class UpdateRoomCommandHandler(
    IUnitOfWork unitOfWork,
    IHotelRepository repository,
    IClock clock,
    IPublisher publisher
) : IRequestHandler<UpdateRoomCommand, Result<RoomDto>>
{
    public async Task<Result<RoomDto>> Handle(
        UpdateRoomCommand command,
        CancellationToken cancellationToken
    )
    {
        if (command.Capacity is null && command.PricePerNight is null)
        {
            return Result<RoomDto>.Failure(Error.Validation("Nothing to update."));
        }

        if (command.Capacity is <= 0)
        {
            return Result<RoomDto>.Failure(Error.Validation("Capacity must be positive."));
        }

        if (command.PricePerNight is < 0)
        {
            return Result<RoomDto>.Failure(Error.Validation("Price cannot be negative."));
        }

        if (command.PricePerNight is not null && string.IsNullOrWhiteSpace(command.Currency))
        {
            return Result<RoomDto>.Failure(
                Error.Validation("Currency is required when price is set.")
            );
        }

        await unitOfWork.BeginAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        try
        {
            var hotel = await repository
                .GetAsync(command.HotelId, cancellationToken)
                .ConfigureAwait(false);
            if (hotel is null)
            {
                await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return Result<RoomDto>.Failure(
                    Error.NotFound(nameof(Domain.Hotel), command.HotelId.Value)
                );
            }

            var room = hotel.Rooms.FirstOrDefault(r => r.Id.Equals(command.RoomId));
            if (room is null)
            {
                await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return Result<RoomDto>.Failure(Error.NotFound(nameof(Room), command.RoomId.Value));
            }

            var price = command.PricePerNight is not null
                ? new Money(command.PricePerNight.Value, command.Currency!)
                : null;

            hotel.UpdateRoom(command.RoomId, price, command.Capacity, clock.UtcNow);

            await repository.UpdateAsync(hotel, cancellationToken).ConfigureAwait(false);
            await publisher
                .PublishDomainEventsAsync(hotel, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result<RoomDto>.Success(RoomDto.From(room));
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
