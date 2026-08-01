using FastEndpoints;

namespace HBP.Hotel.API.Endpoints.Hotels.BulkImportCsv;

internal sealed class BulkImportCsvRequest : IPlainTextRequest
{
    public string Content { get; set; } = string.Empty;
}
