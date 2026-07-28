using System.Reflection;
using FluentAssertions;
using HBP.Persistence.Migrations;
using Microsoft.Extensions.Logging.Abstractions;

namespace HBP.Persistence.UnitTests.Migrations;

public class FluentMigrationRunnerTests
{
    [Fact]
    public void ConstructorThrowsForNullConnectionString()
    {
        var act = () => new FluentMigrationRunner(
            null!,
            new[] { typeof(FluentMigrationRunnerTests).Assembly },
            NullLogger<FluentMigrationRunner>.Instance);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("connectionString");
    }

    [Fact]
    public void ConstructorThrowsForEmptyConnectionString()
    {
        var act = () => new FluentMigrationRunner(
            string.Empty,
            new[] { typeof(FluentMigrationRunnerTests).Assembly },
            NullLogger<FluentMigrationRunner>.Instance);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("connectionString");
    }

    [Fact]
    public void ConstructorThrowsForWhitespaceConnectionString()
    {
        var act = () => new FluentMigrationRunner(
            "   ",
            new[] { typeof(FluentMigrationRunnerTests).Assembly },
            NullLogger<FluentMigrationRunner>.Instance);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("connectionString");
    }

    [Fact]
    public void ConstructorThrowsForNullAssemblies()
    {
        var act = () => new FluentMigrationRunner(
            "Host=localhost",
            null!,
            NullLogger<FluentMigrationRunner>.Instance);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("migrationAssemblies");
    }

    [Fact]
    public void ConstructorThrowsForNullLogger()
    {
        var act = () => new FluentMigrationRunner(
            "Host=localhost",
            new[] { typeof(FluentMigrationRunnerTests).Assembly },
            null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void ConstructorAcceptsValidArguments()
    {
        var act = () => new FluentMigrationRunner(
            "Host=localhost",
            new[] { typeof(FluentMigrationRunnerTests).Assembly },
            NullLogger<FluentMigrationRunner>.Instance);

        act.Should().NotThrow();
    }

    [Fact]
    public void ConstructorAcceptsEmptyAssembliesEnumerable()
    {
        var act = () => new FluentMigrationRunner(
            "Host=localhost",
            Array.Empty<Assembly>(),
            NullLogger<FluentMigrationRunner>.Instance);

        act.Should().NotThrow();
    }
}