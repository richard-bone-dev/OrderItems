using Api.Application.Abstractions;
using Api.Application.Batches.Commands;
using Api.Application.Batches.Dtos;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;

namespace Api.Application.Batches.Commands.Handlers;

public class CreateBatchHandler
    : ICommandHandler<CreateBatchCommand, BatchDto>
{
    private readonly ApplicationDbContext _db;

    public CreateBatchHandler(ApplicationDbContext db) => _db = db;

    public async Task<BatchDto> Handle(CreateBatchCommand cmd, CancellationToken ct = default)
    {
        var batch = Batch.Create(new BatchNumber(cmd.Number));

        await _db.Batches.AddAsync(batch, ct);
        await _db.SaveChangesAsync(ct);

        return new BatchDto(
            batch.Id.Value,
            batch.Number.Value,
            batch.CreatedAt,
            batch.IsActive
        );
    }
}
