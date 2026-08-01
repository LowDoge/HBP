using FastEndpoints;
using HBP.Hotel.Application.Handlers.Hotels.BulkImport;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.BulkImport;

internal sealed class BulkImportHotelsEndpoint(ISender sender)
    : Endpoint<BulkImportHotelsRequest, BulkImportHotelsResponse>
{
    public override void Configure()
    {
        Post("api/{version}/hotels/bulk");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        BulkImportHotelsRequest req,
        CancellationToken cancellationToken
    )
    {
        var items = req
            .Items.Select(i => new BulkHotelItem(i.Name, i.Country, i.City, i.Street, i.PostalCode))
            .ToList();

        var result = await sender.Send(new BulkImportHotelsCommand(items), cancellationToken);
        await Send.OkAsync(
            new BulkImportHotelsResponse(result.Created, result.Failed, result.Errors),
            cancellationToken
        );
    }
}
