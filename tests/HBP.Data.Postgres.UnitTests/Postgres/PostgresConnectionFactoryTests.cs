using FluentAssertions;

namespace HBP.Data.Postgres.UnitTests.Postgres;

public class PostgresConnectionFactoryTests
{
    [Fact]
    public void ConstructorThrowsForNullConnectionString()
    {
        var act = () => new PostgresConnectionFactory(null!);

        act.Should().Throw<ArgumentException>().WithParameterName("connectionString");
    }

    [Fact]
    public void ConstructorThrowsForEmptyConnectionString()
    {
        var act = () => new PostgresConnectionFactory(string.Empty);

        act.Should().Throw<ArgumentException>().WithParameterName("connectionString");
    }

    [Fact]
    public void ConstructorThrowsForWhitespaceConnectionString()
    {
        var act = () => new PostgresConnectionFactory("   ");

        act.Should().Throw<ArgumentException>().WithParameterName("connectionString");
    }

    [Fact]
    public void ConstructorAcceptsValidConnectionString()
    {
        var act = () => new PostgresConnectionFactory("Host=localhost;Database=test");

        act.Should().NotThrow();
    }
}
