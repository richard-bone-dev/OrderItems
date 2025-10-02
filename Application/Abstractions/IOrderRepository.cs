using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions;

public interface IOrderRepository
{
    Task<IReadOnlyCollection<Order>> GetByUserIdAsync(UserId userId, CancellationToken ct = default);
}