using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.SetRoomStatus;

internal sealed class SetRoomStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IHotelRepository repository,
    IClock clock,
    IPublisher publisher
) : IRequestHandler<SetRoomStatusCommand, Result<RoomDto>>
{
    public async Task<Result<RoomDto>> Handle(
        SetRoomStatusCommand command,
        CancellationToken cancellationToken
    )
    {
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

            hotel.SetRoomStatus(command.RoomId, command.Status, clock.UtcNow);

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
