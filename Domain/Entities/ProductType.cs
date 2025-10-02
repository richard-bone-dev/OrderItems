using Api.Domain.Core;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public class ProductType : Entity<ProductTypeId>
{
    public string Name { get; private set; }
    public Money UnitPrice { get; private set; }

    private ProductType() { }

    private ProductType(ProductTypeId id, string name, Money unitPrice)
    {
        Id = id;
        Name = name;
        UnitPrice = unitPrice;
    }

    public static ProductType Create(string name, Money unitPrice)
        => new(ProductTypeId.New(), name, unitPrice);
}