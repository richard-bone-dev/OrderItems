using Api.Domain.Core;

namespace Api.Domain.ValueObjects;

public sealed class UserName : ValueObject
{
    public string Value { get; }

    private UserName() { }

    public UserName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("User name cannot be empty.");

        if (value.Length > 100)
            throw new ArgumentException("User name too long.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}