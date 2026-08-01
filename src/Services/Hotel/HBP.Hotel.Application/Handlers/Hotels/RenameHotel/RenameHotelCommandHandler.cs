using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Hotel.Application.Abstractions;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.RenameHotel;

internal sealed class RenameHotelCommandHandler(
    IUnitOfWork unitOfWork,
    IHotelRepository repository,
    IClock clock,
    IPublisher publisher
) : IRequestHandler<RenameHotelCommand, Result<HotelDto>>
{
    public async Task<Result<HotelDto>> Handle(
        RenameHotelCommand command,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(command.NewName))
        {
            return Result<HotelDto>.Failure(Error.Validation("New name is required."));
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

            hotel.Rename(command.NewName, clock.UtcNow);
            await repository.UpdateAsync(hotel, cancellationToken).ConfigureAwait(false);
            await publisher
                .PublishDomainEventsAsync(hotel, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result<HotelDto>.Success(HotelDto.From(hotel));
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
