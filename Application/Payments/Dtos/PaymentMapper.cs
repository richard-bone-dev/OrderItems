namespace Api.Application.Payments.Dtos;

using Api.Domain.Entities;

public static class PaymentMapper
{
    public static PaymentDto ToDto(Payment payment) => new(
        payment.Id.Value,
        payment.CustomerId.Value,
        payment.PaidAmount.Amount,
        payment.PaymentDate);
}
