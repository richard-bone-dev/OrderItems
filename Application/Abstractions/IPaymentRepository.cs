using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task<IReadOnlyCollection<Payment>> GetAllAsync(CancellationToken ct = default);
    Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Payment>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken ct = default);
}