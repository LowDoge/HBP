namespace HBP.Common;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType()) return false;
        return GetEqualityMembers().SequenceEqual(other.GetEqualityMembers());
    }

    public override bool Equals(object? other) =>
        other is ValueObject v && Equals(v);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var m in GetEqualityMembers()) hash.Add(m);
        return hash.ToHashCode();
    }

    protected abstract IEnumerable<object?> GetEqualityMembers();
}
