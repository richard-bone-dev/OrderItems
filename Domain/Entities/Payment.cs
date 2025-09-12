using Api.Domain.Core;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public class Payment : Entity<PaymentId>
{
    public UserId UserId { get; private set; }
    public Money PaidAmount { get; private set; }
    public Money RemainingAmount { get; private set; }
    public DateTime PaymentDate { get; private set; }

    private Payment() { }

    private Payment(PaymentId paymentId, UserId userId, Money paid, Money remaining, DateTime paymentDate)
    {
        Id = paymentId;
        UserId = userId;
        PaidAmount = paid;
        RemainingAmount = remaining;
        PaymentDate = paymentDate;
    }

    public static Payment Create(UserId userId, Money paid, Money remaining, DateTime? paymentDate = null)
    {
        if (paid.Amount < 0) throw new ArgumentException("Payment amount must be non-negative.");
        return new Payment(PaymentId.New(), userId, paid, remaining, paymentDate ?? DateTime.UtcNow);
    }
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