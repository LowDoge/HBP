namespace HBP.Hotel.API.Endpoints.Hotels.BulkImport;

internal sealed record BulkImportHotelItem(
    string Name,
    string Country,
    string City,
    string Street,
    string? PostalCode
);

internal sealed record BulkImportHotelsRequest(List<BulkImportHotelItem> Items);
