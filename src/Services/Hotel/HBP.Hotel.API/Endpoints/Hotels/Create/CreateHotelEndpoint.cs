using FastEndpoints;
using HBP.Hotel.API.Http;
using HBP.Hotel.Application.Handlers.Hotels.CreateHotel;
using MediatR;

namespace HBP.Hotel.API.Endpoints.Hotels.Create;

internal sealed class CreateHotelEndpoint(ISender sender)
    : Endpoint<CreateHotelRequest, CreateHotelResponse>
{
    public override void Configure()
    {
        Post("api/{version}/hotels");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        CreateHotelRequest req,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateHotelCommand(
            req.Name,
            req.Country,
            req.City,
            req.Street,
            req.PostalCode
        );

        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            await Send.SendProblemAsync(result.Error!, cancellationToken);
            return;
        }

        var hotel = result.Value!;
        await Send.ResponseAsync(
            CreateHotelResponse.From(hotel),
            StatusCodes.Status201Created,
            cancellationToken
        );
    }
}
