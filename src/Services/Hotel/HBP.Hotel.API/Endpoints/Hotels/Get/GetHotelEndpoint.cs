using FastEndpoints;
using HBP.Hotel.API.Http;
using HBP.Hotel.Application.Handlers.Hotels.GetHotel;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.Get;

internal sealed class GetHotelEndpoint(ISender sender) : Endpoint<GetHotelRequest, GetHotelResponse>
{
    public override void Configure()
    {
        Get("api/{version}/hotels/{id}");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetHotelRequest req, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetHotelQuery(HotelId.From(req.Id)), cancellationToken);
        if (result.IsFailure)
        {
            await Send.SendProblemAsync(result.Error!, cancellationToken);
            return;
        }

        await Send.OkAsync(GetHotelResponse.From(result.Value!), cancellationToken);
    }
}
