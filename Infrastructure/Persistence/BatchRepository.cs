using Api.Application.Interfaces;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence;

public class BatchRepository : IBatchRepository
{
    private readonly ApplicationDbContext _db;
    public BatchRepository(ApplicationDbContext db) => _db = db;

    public Batch GetById(BatchId batchId)
        => _db.Batches
              .Single(u => u.Id == batchId);

    public IEnumerable<Batch> GetAll()
        => _db.Batches.AsNoTracking().ToList();

    public void Save(Batch batch)
    {
        var existing = _db.Batches.Find(batch.Id);
        if (existing == null)
            throw new InvalidOperationException("Batch not found.");

        _db.Entry(existing).CurrentValues.SetValues(batch);
        _db.SaveChanges();
    }
}
