using FluentAssertions;

namespace HBP.Common.UnitTests;

public class ValueObjectTests
{
    [Fact]
    public void SameMembersEqualsReturnsTrue()
    {
        var a = new Address { Country = "RU", City = "Moscow" };
        var b = new Address { Country = "RU", City = "Moscow" };

        a.Equals(b).Should().BeTrue();
        b.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void DiffMembersEqualsReturnsFalse()
    {
        var a = new Address { Country = "RU", City = "Moscow" };
        var b = new Address { Country = "RU", City = "Tomsk" };

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void SameInstanceEqualsReturnsTrue()
    {
        var a = new Address { Country = "RU", City = "Moscow" };

        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void NullEqualsReturnsFalse()
    {
        var a = new Address { Country = "RU", City = "Moscow" };

        a.Equals((object?)null).Should().BeFalse();
    }

    [Fact]
    public void DifferentTypesSameMembersEqualsReturnsFalse()
    {
        var a = new Address { Country = "RU", City = "Moscow" };
        var b = new Location { Country = "RU", City = "Tomsk" };

        a.Equals(b).Should().BeFalse();
        b.Equals(a).Should().BeFalse();
    }

    [Fact]
    public void NullableMembersEqualsHandledCorrectly()
    {
        var a = new Person { Name = "Alice", Email = "alice@example.com" };
        var b = new Person { Name = "Alice", Email = null };

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void InstanceGetHashCodeDeterministic()
    {
        var a = new Address { Country = "RU", City = "Moscow" };

        a.GetHashCode().Should().Be(a.GetHashCode());
    }

    [Fact]
    public void SameMembersReturnsEqualsHashCode()
    {
        var a = new Address { Country = "RU", City = "Moscow" };
        var b = new Address { Country = "RU", City = "Moscow" };

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    private sealed class Address : ValueObject
    {
        public string Country { get; init; }
        public string City { get; init; }

        protected override IEnumerable<object?> GetEqualityMembers()
        {
            yield return Country;
            yield return City;
        }
    }

    private sealed class Location : ValueObject
    {
        public string Country { get; init; }
        public string City { get; init; }

        protected override IEnumerable<object?> GetEqualityMembers()
        {
            yield return Country;
            yield return City;
        }
    }

    private sealed class Person : ValueObject
    {
        public string Name { get; init; } = null!;
        public string? Email { get; init; }

        protected override IEnumerable<object?> GetEqualityMembers()
        {
            yield return Name;
            yield return Email;
        }
    }
}
