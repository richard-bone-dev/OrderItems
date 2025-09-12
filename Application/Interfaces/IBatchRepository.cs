using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Interfaces;

public interface IBatchRepository
{
    IEnumerable<Batch> GetAll();
    Batch GetById(BatchId batchId);
    void Save(Batch batch);
}
