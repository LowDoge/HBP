namespace HBP.Hotel.API.Endpoints.Hotels.BulkImportCsv;

internal sealed record BulkImportCsvResponse(int Created, int Failed, List<string> Errors);
