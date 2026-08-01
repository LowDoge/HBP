using FastEndpoints;
using HBP.Hotel.API.Http;
using HBP.Hotel.Application.Handlers.Hotels.RenameHotel;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.Rename;

internal sealed class RenameHotelEndpoint(ISender sender)
    : Endpoint<RenameHotelRequest, RenameHotelResponse>
{
    public override void Configure()
    {
        Put("api/{version}/hotels/{id}");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        RenameHotelRequest req,
        CancellationToken cancellationToken
    )
    {
        var command = new RenameHotelCommand(HotelId.From(req.Id), req.NewName);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            await Send.SendProblemAsync(result.Error!, cancellationToken);
            return;
        }

        await Send.OkAsync(RenameHotelResponse.From(result.Value!), cancellationToken);
    }
}
