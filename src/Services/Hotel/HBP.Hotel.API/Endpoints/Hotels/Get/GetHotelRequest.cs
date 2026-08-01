using FastEndpoints;

namespace HBP.Hotel.API.Endpoints.Hotels.Get;

internal sealed class GetHotelRequest
{
    [BindFrom("id")]
    public Guid Id { get; set; }
}
