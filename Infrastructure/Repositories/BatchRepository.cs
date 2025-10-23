using Api.Application.Abstractions;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class BatchRepository : IBatchRepository
{
    private readonly ApplicationDbContext _db;

    public BatchRepository(ApplicationDbContext db) => _db = db;

    public async Task<Batch?> GetByIdAsync(BatchId id, CancellationToken ct = default)
        => await _db.Batches.Include(b => b.Orders).FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyCollection<Batch>> GetByIdsAsync(IEnumerable<BatchId> ids, CancellationToken ct = default)
        => await _db.Batches
            .Include(b => b.Orders)
            .Where(b => ids.Contains(b.Id))
            .ToListAsync(ct);

    public async Task AddAsync(Batch batch, CancellationToken ct = default)
        => await _db.Batches.AddAsync(batch, ct);

    public async Task<IReadOnlyCollection<Batch>> GetAllAsync(CancellationToken ct = default)
        => await _db.Batches.Include(b => b.Orders).ToListAsync(ct);
}
