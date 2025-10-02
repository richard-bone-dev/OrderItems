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

    public Task AddAsync(Batch batch, CancellationToken ct = default)
        => _db.Batches.AddAsync(batch, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}