using Api.Domain.Core;

namespace Api.Domain.ValueObjects;

public sealed class BatchNumber : ValueObject
{
    public int Value { get; }

    private BatchNumber() { }

    public BatchNumber(int value)
    {
        if (value <= 0) throw new ArgumentException("Batch number must be positive.");
        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
