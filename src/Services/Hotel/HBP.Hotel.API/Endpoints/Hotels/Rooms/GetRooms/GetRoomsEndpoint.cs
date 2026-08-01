using FastEndpoints;
using HBP.Hotel.API.Http;
using HBP.Hotel.Application.Handlers.Hotels.GetRooms;
using HBP.Hotel.Domain;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.Rooms.GetRooms;

internal sealed class GetRoomsEndpoint(ISender sender) : Endpoint<GetRoomsRequest, GetRoomsResponse>
{
    public override void Configure()
    {
        Get("api/{version}/hotels/{id}/rooms");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetRoomsRequest req, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRoomsQuery(HotelId.From(req.Id)), cancellationToken);
        if (result.IsFailure)
        {
            await Send.SendProblemAsync(result.Error!, cancellationToken);
            return;
        }

        await Send.OkAsync(GetRoomsResponse.From(result.Value!), cancellationToken);
    }
}
