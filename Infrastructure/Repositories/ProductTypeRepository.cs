using Api.Application.Abstractions;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class ProductTypeRepository : IProductTypeRepository
{
    private readonly ApplicationDbContext _db;
    public ProductTypeRepository(ApplicationDbContext db) => _db = db;


    public async Task<ProductType?> GetByIdAsync(ProductTypeId id, CancellationToken ct = default)
    => await _db.ProductTypes.FirstOrDefaultAsync(p => p.Id == id, ct);


    public async Task<IReadOnlyCollection<ProductType>> GetAllAsync(CancellationToken ct = default)
    => await _db.ProductTypes.ToListAsync(ct);


    public async Task AddAsync(ProductType productType, CancellationToken ct = default)
    => await _db.ProductTypes.AddAsync(productType, ct);
}