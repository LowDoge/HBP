using FluentAssertions;

namespace HBP.Common.UnitTests;

public class SystemClockTests
{
    [Fact]
    public void SystemClockReturnsUtcNow()
    {
        IClock clock = new SystemClock();
        var before = DateTimeOffset.UtcNow;
        var actual = clock.UtcNow;
        var after = DateTimeOffset.UtcNow;

        actual.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void SystemClockReturnsDateTimeOffsetWithZeroOffset()
    {
        IClock clock = new SystemClock();

        var actual = clock.UtcNow;

        actual.Offset.Should().Be(TimeSpan.Zero);
    }
}
