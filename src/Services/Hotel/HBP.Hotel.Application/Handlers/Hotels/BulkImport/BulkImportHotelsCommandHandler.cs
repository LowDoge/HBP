using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.BulkImport;

internal sealed class BulkImportHotelsCommandHandler(
    IHotelRepository repository,
    IUnitOfWork unitOfWork,
    IClock clock,
    IPublisher publisher
) : IRequestHandler<BulkImportHotelsCommand, BulkImportResult>
{
    public async Task<BulkImportResult> Handle(
        BulkImportHotelsCommand command,
        CancellationToken cancellationToken
    )
    {
        var created = 0;
        var failed = 0;
        var errors = new List<string>();

        foreach (var item in command.Items)
        {
            try
            {
                var hotel = Domain.Hotel.Create(
                    HotelId.New(),
                    item.Name,
                    new Address(item.Country, item.City, item.Street, item.PostalCode),
                    clock.UtcNow
                );

                await unitOfWork
                    .BeginAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                await repository.AddAsync(hotel, cancellationToken).ConfigureAwait(false);
                await publisher
                    .PublishDomainEventsAsync(hotel, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

                created++;
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                failed++;
                errors.Add($"'{item.Name}': {ex.Message}");
            }
        }

        return new BulkImportResult(created, failed, errors);
    }
}
