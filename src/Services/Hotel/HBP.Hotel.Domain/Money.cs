using HBP.Common;

namespace HBP.Hotel.Domain;

public sealed class Money : ValueObject
{
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        }

        Amount = amount;
        Currency = Guard.AgainstNullOrWhiteSpace(currency, nameof(currency));
    }

    public decimal Amount { get; }
    public string Currency { get; }

    protected override IEnumerable<object?> GetEqualityMembers()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
