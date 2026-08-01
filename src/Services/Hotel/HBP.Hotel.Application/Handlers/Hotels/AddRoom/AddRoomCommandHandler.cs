using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.AddRoom;

internal sealed class AddRoomCommandHandler(
    IUnitOfWork unitOfWork,
    IHotelRepository repository,
    IClock clock,
    IPublisher publisher
) : IRequestHandler<AddRoomCommand, Result<HotelDto>>
{
    public async Task<Result<HotelDto>> Handle(
        AddRoomCommand command,
        CancellationToken cancellationToken
    )
    {
        if (command.Capacity <= 0)
        {
            return Result<HotelDto>.Failure(Error.Validation("Capacity must be positive."));
        }

        if (command.PricePerNight < 0)
        {
            return Result<HotelDto>.Failure(Error.Validation("Price cannot be negative."));
        }

        if (string.IsNullOrWhiteSpace(command.Currency))
        {
            return Result<HotelDto>.Failure(Error.Validation("Currency is required."));
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
                return Result<HotelDto>.Failure(
                    Error.NotFound(nameof(Domain.Hotel), command.HotelId.Value)
                );
            }

            hotel.AddRoom(
                command.Type,
                command.Capacity,
                new Money(command.PricePerNight, command.Currency),
                clock.UtcNow
            );

            await repository.UpdateAsync(hotel, cancellationToken).ConfigureAwait(false);
            await publisher
                .PublishDomainEventsAsync(hotel, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result<HotelDto>.Success(HotelDto.From(hotel));
        }
        catch (InvalidOperationException ex)
        {
            await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return Result<HotelDto>.Failure(Error.Conflict(ex.Message));
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
