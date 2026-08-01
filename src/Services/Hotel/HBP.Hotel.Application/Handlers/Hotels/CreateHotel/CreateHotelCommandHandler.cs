using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.CreateHotel;

internal sealed class CreateHotelCommandHandler(
    IUnitOfWork unitOfWork,
    IHotelRepository repository,
    IClock clock,
    IPublisher publisher
) : IRequestHandler<CreateHotelCommand, Result<HotelDto>>
{
    public async Task<Result<HotelDto>> Handle(
        CreateHotelCommand command,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<HotelDto>.Failure(Error.Validation("Hotel name is required."));
        }

        if (
            string.IsNullOrWhiteSpace(command.Country)
            || string.IsNullOrWhiteSpace(command.City)
            || string.IsNullOrWhiteSpace(command.Street)
        )
        {
            return Result<HotelDto>.Failure(
                Error.Validation("Country, city and street are required.")
            );
        }

        await unitOfWork.BeginAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        try
        {
            var hotel = Domain.Hotel.Create(
                HotelId.New(),
                command.Name,
                new Address(command.Country, command.City, command.Street, command.PostalCode),
                clock.UtcNow
            );

            await repository.AddAsync(hotel, cancellationToken).ConfigureAwait(false);
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
