using Api.Application.Abstractions;
using Api.Application.Batches.Dtos;
using Api.Application.Batches.Queries;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Batches.Queries.Handlers;

public class GetCurrentBatchHandler
    : IQueryHandler<GetCurrentBatchQuery, BatchDto?>
{
    private readonly ApplicationDbContext _db;

    public GetCurrentBatchHandler(ApplicationDbContext db) => _db = db;

    public async Task<BatchDto?> Handle(GetCurrentBatchQuery query, CancellationToken ct = default)
    {
        var batch = await _db.Batches
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return batch is null ? null : new BatchDto(
            batch.Id.Value,
            batch.Number.Value,
            batch.CreatedAt,
            batch.IsActive
        );
    }
}
