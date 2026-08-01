using FastEndpoints;
using HBP.Hotel.API.Http;
using HBP.Hotel.Application.Handlers.Hotels.AddRoom;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.AddRoom;

internal sealed class AddRoomEndpoint(ISender sender) : Endpoint<AddRoomRequest, AddRoomResponse>
{
    public override void Configure()
    {
        Post("api/{version}/hotels/{id}/rooms");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddRoomRequest req, CancellationToken cancellationToken)
    {
        var command = new AddRoomCommand(
            HotelId.From(req.Id),
            req.Type,
            req.Capacity,
            req.PricePerNight,
            req.Currency
        );
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            await Send.SendProblemAsync(result.Error!, cancellationToken);
            return;
        }

        await Send.OkAsync(AddRoomResponse.From(result.Value!), cancellationToken);
    }
}
