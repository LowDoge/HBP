using Dapper;
using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Data.Postgres;
using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Domain;

namespace HBP.Hotel.Infrastructure.Persistence;

internal sealed class HotelRepository(IDbContext context, IHotelCache cache)
    : DbRepository(context),
        IHotelRepository
{
    private const string InsertHotelSql = """
        INSERT INTO hotels (id, name, country, city, street, postal_code)
        VALUES (@Id, @Name, @Country, @City, @Street, @PostalCode)
        """;

    private const string InsertRoomSql = """
        INSERT INTO rooms (id, hotel_id, type, capacity, price_per_night, currency, status)
        VALUES (@Id, @HotelId, @Type, @Capacity, @PricePerNight, @Currency, @Status)
        """;

    private const string UpsertRoomSql = """
        INSERT INTO rooms (id, hotel_id, type, capacity, price_per_night, currency, status)
        VALUES (@Id, @HotelId, @Type, @Capacity, @PricePerNight, @Currency, @Status)
        ON CONFLICT (id) DO UPDATE
        SET type = EXCLUDED.type,
            capacity = EXCLUDED.capacity,
            price_per_night = EXCLUDED.price_per_night,
            currency = EXCLUDED.currency,
            status = EXCLUDED.status
        """;

    private const string UpdateHotelSql = """
        UPDATE hotels
        SET name = @Name,
            country = @Country,
            city = @City,
            street = @Street,
            postal_code = @PostalCode,
            updated_at = NOW()
        WHERE id = @Id
        """;

    private const string DeleteMissingRoomsSql = """
        DELETE FROM rooms
        WHERE hotel_id = @HotelId
          AND NOT (id = ANY(@RoomIds))
        """;

    private const string SelectHotelSql = """
        SELECT id           AS "Id",
               name         AS "Name",
               country      AS "Country",
               city         AS "City",
               street       AS "Street",
               postal_code  AS "PostalCode"
        FROM hotels
        WHERE id = @Id
        """;

    private const string SelectRoomsByHotelIdsSql = """
        SELECT id              AS "Id",
               hotel_id        AS "HotelId",
               type            AS "Type",
               capacity        AS "Capacity",
               price_per_night AS "PricePerNight",
               currency        AS "Currency",
               status          AS "Status"
        FROM rooms
        WHERE hotel_id = ANY(@Ids)
        ORDER BY hotel_id, id
        """;

    private const string DeleteHotelSql = """
        DELETE FROM hotels
        WHERE id = @Id
        """;

    private const string ListHotelsSql = """
        SELECT id           AS "Id",
               name         AS "Name",
               country      AS "Country",
               city         AS "City",
               street       AS "Street",
               postal_code  AS "PostalCode"
        FROM hotels
        ORDER BY name
        LIMIT @Take OFFSET @Skip
        """;

    private IHotelCache _cache = Guard.AgainstNull(cache);

    public async Task AddAsync(Domain.Hotel hotel, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(hotel);

        await ExecuteAsync(InsertHotelSql, HotelRow.From(hotel), cancellationToken)
            .ConfigureAwait(false);

        foreach (var room in hotel.Rooms)
        {
            await ExecuteAsync(InsertRoomSql, RoomRow.From(hotel.Id, room), cancellationToken)
                .ConfigureAwait(false);
        }

        await _cache.InvalidateAsync(hotel.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Domain.Hotel hotel, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(hotel);

        await ExecuteAsync(UpdateHotelSql, HotelRow.From(hotel), cancellationToken)
            .ConfigureAwait(false);

        var roomIds = new List<Guid>(hotel.Rooms.Count);
        foreach (var room in hotel.Rooms)
        {
            roomIds.Add(room.Id.Value);
            await ExecuteAsync(UpsertRoomSql, RoomRow.From(hotel.Id, room), cancellationToken)
                .ConfigureAwait(false);
        }

        await ExecuteAsync(
                DeleteMissingRoomsSql,
                new { HotelId = hotel.Id.Value, RoomIds = roomIds },
                cancellationToken
            )
            .ConfigureAwait(false);

        await _cache.InvalidateAsync(hotel.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(HotelId id, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(DeleteHotelSql, new { Id = id.Value }, cancellationToken)
            .ConfigureAwait(false);

        await _cache.InvalidateAsync(id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Domain.Hotel?> GetAsync(
        HotelId id,
        CancellationToken cancellationToken = default
    )
    {
        var hotelRow = await QuerySingleOrDefaultAsync<HotelRow>(
                SelectHotelSql,
                new { Id = id.Value },
                cancellationToken
            )
            .ConfigureAwait(false);

        if (hotelRow is null)
        {
            return null;
        }

        var roomRows = await QueryAsync<RoomRow>(
                SelectRoomsByHotelIdsSql,
                new { Ids = new[] { hotelRow.Id } },
                cancellationToken
            )
            .ConfigureAwait(false);

        return ToDomain(hotelRow, roomRows);
    }

    public async Task<IReadOnlyList<Domain.Hotel>> ListAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        var hotelRows = await QueryAsync<HotelRow>(
                ListHotelsSql,
                new { Skip = skip, Take = take },
                cancellationToken
            )
            .ConfigureAwait(false);

        var hotelRowList = hotelRows.AsList();
        if (hotelRowList.Count == 0)
        {
            return Array.Empty<Domain.Hotel>();
        }

        var roomRows = await QueryAsync<RoomRow>(
                SelectRoomsByHotelIdsSql,
                new { Ids = hotelRowList.Select(h => h.Id).ToList() },
                cancellationToken
            )
            .ConfigureAwait(false);

        var roomsRowsByHotel = roomRows
            .GroupBy(r => r.HotelId)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());

        return hotelRowList
            .Select(hotel =>
            {
                roomsRowsByHotel.TryGetValue(hotel.Id, out var hotelRooms);
                return ToDomain(hotel, hotelRooms ?? Enumerable.Empty<RoomRow>());
            })
            .ToList();
    }

    private static Domain.Hotel ToDomain(HotelRow row, IEnumerable<RoomRow> roomRows)
    {
        var rooms = roomRows.Select(r => r.ToRoom()).ToList();
        return Domain.Hotel.Reconstitute(
            HotelId.From(row.Id),
            row.Name,
            new Address(row.Country, row.City, row.Street, row.PostalCode),
            rooms
        );
    }
}
