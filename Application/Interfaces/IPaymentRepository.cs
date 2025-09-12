using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Interfaces;

public interface IPaymentRepository
{
    Payment GetById(PaymentId paymentId);
    IEnumerable<Payment> GetByUserId(UserId userId);
    void Save(Payment payment);
}