using Api.Domain.ValueObjects;

namespace Api.Domain.Events;

// --- Domain Service for Batch Assignment ---
public interface IBatchAssignmentService
{
    BatchNumber GetCurrentBatch();
    void AdvanceToNextBatch();
}
