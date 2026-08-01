using FastEndpoints;
using HBP.Common;
using HBP.Hotel.API.Http;
using HBP.Hotel.Application.Handlers.Hotels.BulkImport;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.BulkImportCsv;

internal sealed class BulkImportCsvEndpoint(ISender sender)
    : Endpoint<BulkImportCsvRequest, BulkImportCsvResponse>
{
    public override void Configure()
    {
        Post("api/{version}/hotels/bulk/csv");
        Version(1);
        AllowAnonymous();
        Description(x => x.Accepts<BulkImportCsvRequest>("text/csv", "text/plain"));
    }

    public override async Task HandleAsync(
        BulkImportCsvRequest req,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<BulkHotelItem> items;
        try
        {
            items = CsvHotelParser.Parse(req.Content);
        }
        catch (FormatException ex)
        {
            await Send.SendProblemAsync(Error.Validation(ex.Message), cancellationToken);
            return;
        }

        var result = await sender.Send(
            new BulkImportHotelsCommand(items.ToList()),
            cancellationToken
        );

        await Send.OkAsync(
            new BulkImportCsvResponse(result.Created, result.Failed, result.Errors),
            cancellationToken
        );
    }
}
