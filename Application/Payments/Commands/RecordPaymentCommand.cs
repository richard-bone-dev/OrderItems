public record RecordPaymentCommand(
    Guid UserId,
    decimal Amount,
    DateTime? PaymentDate = null);
