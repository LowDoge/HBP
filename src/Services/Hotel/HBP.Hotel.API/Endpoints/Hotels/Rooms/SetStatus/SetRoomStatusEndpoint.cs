using FastEndpoints;
using HBP.Hotel.API.Http;
using HBP.Hotel.Application.Handlers.Hotels.SetRoomStatus;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.SetStatus;

internal sealed class SetRoomStatusEndpoint(ISender sender)
    : Endpoint<SetRoomStatusRequest, SetRoomStatusResponse>
{
    public override void Configure()
    {
        Patch("api/{version}/hotels/{id}/rooms/{roomId}/status");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        SetRoomStatusRequest req,
        CancellationToken cancellationToken
    )
    {
        var command = new SetRoomStatusCommand(
            HotelId.From(req.Id),
            RoomId.From(req.RoomId),
            req.Status
        );
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            await Send.SendProblemAsync(result.Error!, cancellationToken);
            return;
        }

        await Send.OkAsync(SetRoomStatusResponse.From(result.Value!), cancellationToken);
    }
}
