using FastEndpoints;
using HBP.Hotel.Application.Handlers.Hotels.ListHotels;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.List;

internal sealed class ListHotelsEndpoint(ISender sender)
    : Endpoint<ListHotelsRequest, ListHotelsResponse>
{
    public override void Configure()
    {
        Get("api/{version}/hotels");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        ListHotelsRequest req,
        CancellationToken cancellationToken
    )
    {
        var hotels = await sender.Send(new ListHotelsQuery(req.Skip, req.Take), cancellationToken);
        await Send.OkAsync(ListHotelsResponse.From(hotels), cancellationToken);
    }
}
