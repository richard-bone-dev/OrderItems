namespace Api.Domain.Core;

public abstract class StronglyTypedIntId<T> : IEquatable<StronglyTypedIntId<T>>
    where T : StronglyTypedIntId<T>
{
    public int Value { get; }

    protected StronglyTypedIntId(int value)
    {
        if (value <= 0) throw new ArgumentException($"{typeof(T).Name} must be positive.");
        Value = value;
    }

    public override string ToString() => Value.ToString();

    public bool Equals(StronglyTypedIntId<T>? other) => other?.Value == Value;
    public override bool Equals(object? obj) => obj is StronglyTypedIntId<T> other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();

    public static implicit operator int(StronglyTypedIntId<T> id) => id.Value;
}