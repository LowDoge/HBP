using HBP.Common;

namespace HBP.Hotel.Domain;

public sealed class Address : ValueObject
{
    public Address(string country, string city, string street, string? postalCode = null)
    {
        Country = Guard.AgainstNullOrWhiteSpace(country, nameof(country));
        City = Guard.AgainstNullOrWhiteSpace(city, nameof(city));
        Street = Guard.AgainstNullOrWhiteSpace(street, nameof(street));
        PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode;
    }

    public string Country { get; }
    public string City { get; }
    public string Street { get; }
    public string? PostalCode { get; }

    protected override IEnumerable<object?> GetEqualityMembers()
    {
        yield return Country;
        yield return City;
        yield return Street;
        yield return PostalCode;
    }

    public override string ToString() =>
        PostalCode is null
            ? $"{Country}, {City}, {Street}"
            : $"{Country}, {City}, {Street}, {PostalCode}";
}
