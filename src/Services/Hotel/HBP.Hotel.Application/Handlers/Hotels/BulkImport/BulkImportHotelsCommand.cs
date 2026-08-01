using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.BulkImport;

public sealed record BulkHotelItem(
    string Name,
    string Country,
    string City,
    string Street,
    string? PostalCode
);

public sealed record BulkImportResult(int Created, int Failed, List<string> Errors);

public sealed record BulkImportHotelsCommand(List<BulkHotelItem> Items)
    : IRequest<BulkImportResult>;
