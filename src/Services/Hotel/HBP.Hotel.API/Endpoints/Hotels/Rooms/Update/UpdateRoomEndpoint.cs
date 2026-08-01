using FastEndpoints;
using HBP.Hotel.API.Http;
using HBP.Hotel.Application.Handlers.Hotels.UpdateRoom;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.Update;

internal sealed class UpdateRoomEndpoint(ISender sender)
    : Endpoint<UpdateRoomRequest, UpdateRoomResponse>
{
    public override void Configure()
    {
        Put("api/{version}/hotels/{id}/rooms/{roomId}");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        UpdateRoomRequest req,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateRoomCommand(
            HotelId.From(req.Id),
            RoomId.From(req.RoomId),
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

        await Send.OkAsync(UpdateRoomResponse.From(result.Value!), cancellationToken);
    }
}
