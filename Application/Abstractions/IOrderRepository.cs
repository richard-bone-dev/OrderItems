using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions;

public interface IOrderRepository
{
    Task<IReadOnlyCollection<Order>> GetByUserIdAsync(CustomerId userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Order>> GetByBatchIdAsync(BatchId batchId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
}