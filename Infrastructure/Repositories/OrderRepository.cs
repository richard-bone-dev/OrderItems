using Api.Application.Abstractions;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;

    public OrderRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken ct = default)
    {
        return await _db.Orders
            .Where(o => o.CustomerId == customerId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Order>> GetByBatchIdAsync(BatchId batchId, CancellationToken ct = default)
    {
        return await _db.Orders
            .Where(o => o.BatchId == batchId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Orders
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _db.Orders.AddAsync(order, ct);
    }
}