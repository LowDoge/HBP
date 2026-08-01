using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Hotel.Application.Abstractions;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.DeleteHotel;

internal sealed class DeleteHotelCommandHandler(IUnitOfWork unitOfWork, IHotelRepository repository)
    : IRequestHandler<DeleteHotelCommand, Result>
{
    public async Task<Result> Handle(
        DeleteHotelCommand command,
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
                return Result.Failure(Error.NotFound(nameof(Domain.Hotel), command.HotelId.Value));
            }

            await repository.DeleteAsync(command.HotelId, cancellationToken).ConfigureAwait(false);
            await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
