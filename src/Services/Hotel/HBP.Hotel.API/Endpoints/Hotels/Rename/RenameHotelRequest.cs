using FastEndpoints;

namespace HBP.Hotel.API.Endpoints.Hotels.Rename;

internal sealed class RenameHotelRequest
{
    [BindFrom("id")]
    public Guid Id { get; set; }

    public string NewName { get; set; } = string.Empty;
}
