using Api.Application.Interfaces;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence;

public class ProductTypeRepository : IProductTypeRepository
{
    private readonly ApplicationDbContext _db;
    public ProductTypeRepository(ApplicationDbContext db) => _db = db;

    public ProductType GetById(ProductTypeId productTypeId)
        => _db.ProductTypes
              .Single(u => u.Id == productTypeId);

    public IEnumerable<ProductType> GetAll()
        => _db.ProductTypes.AsNoTracking().ToList();

    public void Save(ProductType productType)
    {
        var existing = _db.ProductTypes.Find(productType.Id);
        if (existing == null)
            throw new InvalidOperationException("ProductType not found.");

        _db.Entry(existing).CurrentValues.SetValues(productType);
        _db.SaveChanges();
    }
}