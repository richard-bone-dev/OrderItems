using Api.Domain.Core;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public class Batch : Entity<BatchId>
{
    public BatchNumber BatchNumber { get; }
    public DateTime CreatedDate { get; private set; }

    private Batch() { }

    private Batch(BatchId batchId, BatchNumber batchNumber, DateTime createdDate)
    {
        Id = batchId;
        BatchNumber = batchNumber;
        CreatedDate = createdDate;
    }

    public static Batch Create(BatchNumber batchNumber, DateTime createdDate)
    {
        return new Batch(BatchId.New(), batchNumber, createdDate);
    }
}