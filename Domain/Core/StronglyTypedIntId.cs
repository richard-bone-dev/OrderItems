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


//public class PaymentStatus : Entity<PaymentStatusId>
//{
//    public PaymentId PaymentId { get; private set; }
//    public PaymentState State { get; private set; }
//    public DateTime ChangedAt { get; private set; }
//    public string? Reason { get; private set; }
//    public string? ExternalReference { get; private set; }

//    private PaymentStatus() { }

//    private PaymentStatus(PaymentStatusId id, PaymentState state, DateTime changedAt, string? reason = null, string? externalReference = null)
//    {
//        Id = id;
//        State = state;
//        ChangedAt = changedAt;
//        Reason = reason;
//        ExternalReference = externalReference;
//    }

//    public static PaymentStatus Pending() =>
//        new PaymentStatus(PaymentStatusId.New(), PaymentState.Pending, DateTime.UtcNow);

//    public static PaymentStatus Settled(DateTime settledAt, string? externalReference = null) =>
//        new PaymentStatus(PaymentStatusId.New(), PaymentState.Settled, settledAt, null, externalReference);

//    public static PaymentStatus Failed(string reason) =>
//        new PaymentStatus(PaymentStatusId.New(), PaymentState.Failed, DateTime.UtcNow, reason);

//    public static PaymentStatus Refunded(DateTime refundedAt, string? reason = null) =>
//        new PaymentStatus(PaymentStatusId.New(), PaymentState.Refunded, refundedAt, reason);
//}

// --- Domain Events ---

//public class StronglyTypedIdJsonConverter<T> : JsonConverter<T> where T : StronglyTypedId<T>
//{
//    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//        => (T)Activator.CreateInstance(typeof(T), reader.GetGuid())!;

//    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
//        => writer.WriteStringValue(value.Value);
//}

//public class StronglyTypedIntIdJsonConverter<T> : JsonConverter<T> where T : StronglyTypedIntId<T>
//{
//    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//        => (T)Activator.CreateInstance(typeof(T), reader.GetInt32())!;

//    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
//        => writer.WriteNumberValue(value.Value);
//}