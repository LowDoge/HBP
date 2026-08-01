using FastEndpoints;
using HBP.Hotel.API.Http;
using HBP.Hotel.Application.Handlers.Hotels.DeleteHotel;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.Delete;

internal sealed class DeleteHotelEndpoint(ISender sender)
    : Endpoint<DeleteHotelRequest, DeleteHotelResponse>
{
    public override void Configure()
    {
        Delete("api/{version}/hotels/{id}");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        DeleteHotelRequest req,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new DeleteHotelCommand(HotelId.From(req.Id)),
            cancellationToken
        );
        if (result.IsSuccess)
        {
            await Send.SendProblemAsync(result.Error!, cancellationToken);
            return;
        }

        await Send.NoContentAsync(cancellationToken);
    }
}
