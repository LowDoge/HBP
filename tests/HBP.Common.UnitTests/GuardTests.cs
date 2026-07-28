using FluentAssertions;

namespace HBP.Common.UnitTests;

public class GuardTests
{
    [Fact]
    public void AgainstNullReturnsValueForNonNullReference()
    {
        var obj = new object();
        var result = Guard.AgainstNull(obj, nameof(obj));

        result.Should().BeSameAs(obj);
    }

    [Fact]
    public void AgainstNullThrowsForNullReference()
    {
        object? obj = null;
        var act = () => Guard.AgainstNull(obj, nameof(obj));

        act.Should().Throw<ArgumentNullException>().WithParameterName("obj");
    }

    [Fact]
    public void AgainstNullReturnsValueForHasValueNullable()
    {
        int? value = 42;
        var result = Guard.AgainstNull(value, nameof(value));

        result.Should().Be(42);
    }

    [Fact]
    public void AgainstNullThrowsForNullNullable()
    {
        int? value = null;
        var act = () => Guard.AgainstNull(value, nameof(value));

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("a")]
    [InlineData("  text  ")]
    public void AgainstNullOrEmptyReturnsValueForValidString(string input)
    {
        var result = Guard.AgainstNullOrEmpty(input, nameof(input));

        result.Should().Be(input);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void AgainstNullOrEmptyThrowsForInvalidString(string? input)
    {
        var act = () => Guard.AgainstNullOrEmpty(input, nameof(input));

        act.Should().Throw<ArgumentException>().WithParameterName("input");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void AgainstNegativeReturnsValueForNonNegative(int input)
    {
        var result = Guard.AgainstNegative(input, nameof(input));

        result.Should().Be(input);
    }

    [Fact]
    public void AgainstNegativeThrowsForNegative()
    {
        var act = () => Guard.AgainstNegative(-1, "input");

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("input");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void AgainstNonPositiveReturnsValueForPositive(int input)
    {
        var result = Guard.AgainstNonPositive(input, nameof(input));

        result.Should().Be(input);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void AgainstNonPositiveThrowsForNonPositive(int input)
    {
        var act = () => Guard.AgainstNonPositive(input, nameof(input));

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("input");
    }

    [Fact]
    public void AgainstThrowsForTrueCondition()
    {
        var act = () => Guard.Against(true, "custom message");

        act.Should().Throw<ArgumentException>().WithMessage("custom message*");
    }

    [Fact]
    public void AgainstDoesNotThrowForFalseCondition()
    {
        var act = () => Guard.Against(false, "should not see this");

        act.Should().NotThrow();
    }
}
