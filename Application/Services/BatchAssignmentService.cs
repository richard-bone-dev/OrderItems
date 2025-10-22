using Api.Domain.Events;
using Api.Domain.ValueObjects;

namespace Api.Application.Services;

public class BatchAssignmentService : IBatchAssignmentService
{
    private int _current = 2;
    public BatchNumber GetCurrentBatch() => new BatchNumber(_current);
    public void AdvanceToNextBatch() => _current++;
}