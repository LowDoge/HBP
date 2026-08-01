using FluentAssertions;

namespace HBP.Common.UnitTests;

public class EntityTests
{
    [Fact]
    public void SameIdEqualsReturnsTrue()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void NullEqualsReturnsFalse()
    {
        var a = new TestEntity(Guid.NewGuid());

        a.Equals((object?)null).Should().BeFalse();
        (a == (object?)null).Should().BeFalse();
    }

    [Fact]
    public void InstanceGetHashCodeDeterministic()
    {
        var a = new TestEntity(Guid.NewGuid());

        a.GetHashCode().Should().Be(a.GetHashCode());
    }

    [Fact]
    public void SameIdReturnsEqualsHashCode()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void HashSetContainsEntity()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var set = new HashSet<TestEntity> { a };

        set.Contains(a).Should().BeTrue();
        set.Contains(new TestEntity(id)).Should().BeTrue();
    }

    private sealed class TestEntity(Guid id) : Entity<Guid>(id) { }
}
