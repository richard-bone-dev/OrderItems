using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions;

public interface IProductTypeRepository
{
    ProductType GetById(ProductTypeId productType);
    IEnumerable<ProductType> GetAll();
    void Save(ProductType productType);
}
