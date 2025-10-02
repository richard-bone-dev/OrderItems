using Api.Domain.Core;

namespace Api.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    private Money() { } // EF

    public Money(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Money cannot be negative.");
        Amount = amount;
    }

    public Money Add(Money other) => new(Amount + other.Amount);
    public Money Subtract(Money other) => new(Amount - other.Amount);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString() => Amount.ToString("C2");
}
