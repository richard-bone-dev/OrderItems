namespace Api.Application.Payments.Commands;

public record RecordPaymentCommand(
    Guid CustomerId,
    decimal Amount,
    DateTime? PaymentDate = null);
