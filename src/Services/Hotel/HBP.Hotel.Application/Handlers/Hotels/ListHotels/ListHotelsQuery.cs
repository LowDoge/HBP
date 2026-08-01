using MediatR;

namespace HBP.Hotel.Application.Handlers.Hotels.ListHotels;

public sealed record ListHotelsQuery(int Skip = 0, int Take = 50)
    : IRequest<IReadOnlyList<HotelDto>>;
