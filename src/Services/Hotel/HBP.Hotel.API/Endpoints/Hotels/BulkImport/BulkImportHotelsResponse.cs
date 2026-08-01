namespace HBP.Hotel.API.Endpoints.Hotels.BulkImport;

internal sealed record BulkImportHotelsResponse(int Created, int Failed, List<string> Errors);
