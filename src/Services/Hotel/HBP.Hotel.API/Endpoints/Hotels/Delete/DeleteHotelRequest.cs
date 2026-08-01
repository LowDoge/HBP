using FastEndpoints;

namespace HBP.Hotel.API.Endpoints.Hotels.Delete;

internal sealed class DeleteHotelRequest
{
    [BindFrom("id")]
    public Guid Id { get; set; }
}
