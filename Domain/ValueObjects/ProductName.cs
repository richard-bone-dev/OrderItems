using Api.Domain.Core;

namespace Api.Domain.ValueObjects;

public sealed class ProductName : ValueObject
{
    public string Value { get; }

    private ProductName() { }

    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product name cannot be empty.");

        if (value.Length > 100)
            throw new ArgumentException("Product name too long.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}