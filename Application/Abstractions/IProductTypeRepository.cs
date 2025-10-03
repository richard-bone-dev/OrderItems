// Shared/FakeRepositories/FakeUserRepository.cs
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions;

public interface IProductTypeRepository
{
    Task AddAsync(ProductType type, CancellationToken ct = default);
    Task<ProductType?> GetByIdAsync(ProductTypeId id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
