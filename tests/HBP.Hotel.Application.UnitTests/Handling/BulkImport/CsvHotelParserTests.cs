using FluentAssertions;
using HBP.Hotel.Application.Handlers.Hotels.BulkImport;

namespace HBP.Hotel.Application.UnitTests.Handling.BulkImport;

public class CsvHotelParserTests
{
    [Fact]
    public void Parse_WhenValidCsv_ReturnsItems()
    {
        var csv = """
            Grand Hotel,US,New York,5th Ave,10001
            Beach Resort,ES,Malaga,Paseo Maritimo,29001
            """;

        var items = CsvHotelParser.Parse(csv);

        items.Should().HaveCount(2);
        items[0]
            .Should()
            .Be(new BulkHotelItem("Grand Hotel", "US", "New York", "5th Ave", "10001"));
        items[1]
            .Should()
            .Be(new BulkHotelItem("Beach Resort", "ES", "Malaga", "Paseo Maritimo", "29001"));
    }

    [Fact]
    public void Parse_WhenEmptyPostalCode_ReturnsNullPostalCode()
    {
        var csv = "Grand Hotel,US,New York,5th Ave,";

        var items = CsvHotelParser.Parse(csv);

        items.Should().ContainSingle();
        items[0].PostalCode.Should().BeNull();
    }

    [Fact]
    public void Parse_WhenBlankLinesAreIgnored_ReturnsItems()
    {
        var csv =
            "Grand Hotel,US,New York,5th Ave,10001\n\n   \nBeach Resort,ES,Malaga,Seafront,29001";

        var items = CsvHotelParser.Parse(csv);

        items.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_WhenLineHasWrongColumnCount_Throws()
    {
        var csv = "Grand Hotel,US,New York";

        var act = () => CsvHotelParser.Parse(csv);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Parse_WhenEmptyInput_ReturnsEmpty()
    {
        CsvHotelParser.Parse("").Should().BeEmpty();
    }
}
