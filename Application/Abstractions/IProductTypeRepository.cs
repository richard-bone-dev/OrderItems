using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions;

public interface IProductTypeRepository
{
    Task AddAsync(ProductType productType, CancellationToken ct = default);
    Task<IReadOnlyCollection<ProductType>> GetAllAsync(CancellationToken ct = default);
    Task<ProductType?> GetByIdAsync(ProductTypeId id, CancellationToken ct = default);
}
