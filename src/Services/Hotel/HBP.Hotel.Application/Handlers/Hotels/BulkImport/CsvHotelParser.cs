namespace HBP.Hotel.Application.Handlers.Hotels.BulkImport;

public static class CsvHotelParser
{
    public static IReadOnlyList<BulkHotelItem> Parse(string csv)
    {
        ArgumentNullException.ThrowIfNull(csv);

        var items = new List<BulkHotelItem>();
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var columns = line.Split(',', StringSplitOptions.TrimEntries);
            if (columns.Length != 5)
            {
                throw new FormatException(
                    $"Line {i + 1}: expected 5 columns (Name,Country,City,Street,PostalCode), got {columns.Length}."
                );
            }

            items.Add(
                new BulkHotelItem(
                    columns[0],
                    columns[1],
                    columns[2],
                    columns[3],
                    string.IsNullOrWhiteSpace(columns[4]) ? null : columns[4]
                )
            );
        }

        return items;
    }
}
