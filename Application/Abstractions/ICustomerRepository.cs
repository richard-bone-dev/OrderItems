using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken ct = default);
    Task AddAsync(Customer user, CancellationToken ct = default);
    Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken ct = default);
}
