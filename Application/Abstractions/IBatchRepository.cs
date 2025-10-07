using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions;

public interface IBatchRepository
{
    Task<Batch?> GetByIdAsync(BatchId id, CancellationToken ct = default);
    Task AddAsync(Batch batch, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<Batch>> GetByIdsAsync(IEnumerable<BatchId> ids, CancellationToken ct = default);
    Task<IReadOnlyCollection<Batch>> GetAllAsync(CancellationToken ct = default);
}