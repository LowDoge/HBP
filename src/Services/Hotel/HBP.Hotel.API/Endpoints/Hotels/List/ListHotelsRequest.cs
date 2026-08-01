using FastEndpoints;

namespace HBP.Hotel.API.Endpoints.Hotels.List;

internal sealed class ListHotelsRequest
{
    [QueryParam]
    public int Skip { get; set; }

    [QueryParam]
    public int Take { get; set; } = 50;
}
