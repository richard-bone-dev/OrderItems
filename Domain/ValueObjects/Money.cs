namespace Api.Domain.ValueObjects;

public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public Money(decimal amount)
    {
        //if (amount < 0) throw new ArgumentException("Money amount cannot be negative.");
        Amount = amount;
    }

    public Money Add(Money other)
    {
        return new Money(Amount + other.Amount);
    }

    public Money Subtract(Money other)
    {
        return new Money(Amount - other.Amount);
    }

    public bool Equals(Money other) => other is not null && Amount == other.Amount;
    public override bool Equals(object obj) => Equals(obj as Money);
    public override int GetHashCode() => HashCode.Combine(Amount);
}
