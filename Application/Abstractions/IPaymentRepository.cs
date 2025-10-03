// Shared/FakeRepositories/FakeUserRepository.cs
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Payment>> GetByUserIdAsync(UserId userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
