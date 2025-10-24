using Api.Application.Payments.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Interfaces;

public interface IPaymentService
{
    MakePaymentResponse MakePayment(UserId userId, MakePaymentRequest request);
    IEnumerable<PaymentDto> GetUserPayments(UserId userId);
}