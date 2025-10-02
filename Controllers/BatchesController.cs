using Api.Application.Batches.Commands;
using Api.Application.Batches.Commands.Handlers;
using Api.Application.Batches.Dtos;
using Api.Application.Batches.Queries;
using Api.Application.Batches.Queries.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/batches")]
public class BatchesController : ControllerBase
{
    private readonly CreateBatchHandler _createBatch;
    private readonly GetCurrentBatchHandler _getCurrent;

    public BatchesController(
        CreateBatchHandler createBatch,
        GetCurrentBatchHandler getCurrent)
    {
        _createBatch = createBatch;
        _getCurrent = getCurrent;
    }

    [HttpPost]
    public async Task<ActionResult<BatchDto>> Create([FromBody] CreateBatchCommand cmd, CancellationToken ct)
        => Ok(await _createBatch.Handle(cmd, ct));

    [HttpGet("current")]
    public async Task<ActionResult<BatchDto?>> GetCurrent(CancellationToken ct)
        => Ok(await _getCurrent.Handle(new GetCurrentBatchQuery(), ct));
}