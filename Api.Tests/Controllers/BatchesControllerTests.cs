using Api.Application.Abstractions;
using Api.Application.Batches.Commands;
using Api.Application.Batches.Commands.Handlers;
using Api.Application.Batches.Dtos;
using Api.Application.Batches.Queries;
using Api.Application.Batches.Queries.Handlers;
using Api.Controllers;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Xunit;

namespace Api.Tests.Controllers;

public class BatchesControllerTests
{
    [Fact]
    public async Task CreateAsync_ShouldReturnOkWithBatchDto_AndPersistBatch()
    {
        var repository = new RecordingBatchRepository();
        await using var dbContext = CreateContext();
        var controller = new BatchesController(
            new CreateBatchHandler(repository),
            new GetCurrentBatchHandler(dbContext));

        var command = new CreateBatchCommand(7);
        using var cts = new CancellationTokenSource();

        var result = await controller.CreateAsync(command, cts.Token);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<BatchDto>().Subject;

        dto.Number.Should().Be(command.Number);
        repository.AddedBatch.Should().NotBeNull();
        repository.AddedBatch!.Number.Value.Should().Be(command.Number);
        repository.CapturedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task CreateAsync_WhenCancellationRequestedBeforeCall_ShouldThrowTaskCanceled()
    {
        var repository = new RecordingBatchRepository();
        await using var dbContext = CreateContext();
        var controller = new BatchesController(
            new CreateBatchHandler(repository),
            new GetCurrentBatchHandler(dbContext));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await controller.CreateAsync(new CreateBatchCommand(3), cts.Token);

        await act.Should().ThrowAsync<TaskCanceledException>();
        repository.CapturedToken.Should().Be(cts.Token);
        repository.CancellationObserved.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentAsync_ShouldReturnLatestActiveBatch()
    {
        await using var dbContext = CreateContext(nameof(GetCurrentAsync_ShouldReturnLatestActiveBatch));
        var controller = new BatchesController(
            new CreateBatchHandler(new NoOpBatchRepository()),
            new GetCurrentBatchHandler(dbContext));

        var inactive = Batch.Create(new BatchNumber(1));
        inactive.Close();
        SetCreatedAt(inactive, DateTime.UtcNow.AddHours(-2));

        var olderActive = Batch.Create(new BatchNumber(2));
        SetCreatedAt(olderActive, DateTime.UtcNow.AddHours(-1));

        var latestActive = Batch.Create(new BatchNumber(3));
        SetCreatedAt(latestActive, DateTime.UtcNow);

        await dbContext.Batches.AddRangeAsync(inactive, olderActive, latestActive);
        await dbContext.SaveChangesAsync();

        var result = await controller.GetCurrentAsync(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<BatchDto>().Subject;

        dto.Id.Should().Be(latestActive.Id.Value);
        dto.Number.Should().Be(latestActive.Number.Value);
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentAsync_WhenNoActiveBatchExists_ShouldReturnOkWithNull()
    {
        await using var dbContext = CreateContext(nameof(GetCurrentAsync_WhenNoActiveBatchExists_ShouldReturnOkWithNull));
        var controller = new BatchesController(
            new CreateBatchHandler(new NoOpBatchRepository()),
            new GetCurrentBatchHandler(dbContext));

        var inactive = Batch.Create(new BatchNumber(10));
        inactive.Close();
        await dbContext.Batches.AddAsync(inactive);
        await dbContext.SaveChangesAsync();

        var result = await controller.GetCurrentAsync(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentAsync_WhenCancellationRequested_ShouldThrowTaskCanceled()
    {
        await using var dbContext = CreateContext(nameof(GetCurrentAsync_WhenCancellationRequested_ShouldThrowTaskCanceled));
        var controller = new BatchesController(
            new CreateBatchHandler(new NoOpBatchRepository()),
            new GetCurrentBatchHandler(dbContext));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await controller.GetCurrentAsync(cts.Token);

        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    private static ApplicationDbContext CreateContext(string? name = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void SetCreatedAt(Batch batch, DateTime value)
    {
        typeof(Batch)
            .GetProperty(nameof(Batch.CreatedAt))!
            .SetValue(batch, value);
    }

    private sealed class RecordingBatchRepository : IBatchRepository
    {
        public Batch? AddedBatch { get; private set; }
        public CancellationToken CapturedToken { get; private set; }
        public bool CancellationObserved { get; private set; }

        public Task AddAsync(Batch batch, CancellationToken ct = default)
        {
            AddedBatch = batch;
            CapturedToken = ct;
            CancellationObserved = ct.IsCancellationRequested;

            return ct.IsCancellationRequested
                ? Task.FromCanceled(ct)
                : Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Batch>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyCollection<Batch>>(Array.Empty<Batch>());

        public Task<Batch?> GetByIdAsync(BatchId id, CancellationToken ct = default)
            => Task.FromResult<Batch?>(null);

        public Task<IReadOnlyCollection<Batch>> GetByIdsAsync(IEnumerable<BatchId> ids, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyCollection<Batch>>(Array.Empty<Batch>());
    }

    private sealed class NoOpBatchRepository : IBatchRepository
    {
        public Task AddAsync(Batch batch, CancellationToken ct = default) => Task.CompletedTask;

        public Task<IReadOnlyCollection<Batch>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyCollection<Batch>>(Array.Empty<Batch>());

        public Task<Batch?> GetByIdAsync(BatchId id, CancellationToken ct = default)
            => Task.FromResult<Batch?>(null);

        public Task<IReadOnlyCollection<Batch>> GetByIdsAsync(IEnumerable<BatchId> ids, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyCollection<Batch>>(Array.Empty<Batch>());
    }
}
