using HBP.Hotel.API.Endpoints.Hotels.Get;
using HBP.Hotel.Application;

namespace HBP.Hotel.API.Endpoints.Hotels.List;

internal sealed record ListHotelsResponse(IReadOnlyList<GetHotelResponse> Hotels)
{
    public static ListHotelsResponse From(IEnumerable<HotelDto> hotels) =>
        new(hotels.Select(GetHotelResponse.From).ToList());
}
