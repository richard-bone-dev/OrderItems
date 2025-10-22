using Api.Domain.Core;

namespace Api.Domain.ValueObjects;

public sealed class CustomerName : ValueObject
{
    public string Value { get; }

    private CustomerName() { }

    public CustomerName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Customer name cannot be empty.");

        if (value.Length > 100)
            throw new ArgumentException("Customer name too long.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}