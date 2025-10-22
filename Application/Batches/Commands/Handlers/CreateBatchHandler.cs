using Api.Application.Abstractions;
using Api.Application.Batches.Dtos;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Batches.Commands.Handlers;

public class CreateBatchHandler : ICommandHandlerAsync<CreateBatchCommand, BatchDto>
{
    private readonly IBatchRepository _repo;

    public CreateBatchHandler(IBatchRepository repo) => _repo = repo;

    public async Task<BatchDto> HandleAsync(CreateBatchCommand cmd, CancellationToken ct = default)
    {
        var batch = Batch.Create(new BatchNumber(cmd.Number));
        await _repo.AddAsync(batch, ct);

        return new BatchDto(
            batch.Id.Value,
            batch.Number.Value,
            batch.CreatedAt,
            batch.IsActive
        );
    }
}