namespace Api.Application.ProductTypes.Dtos;

using Api.Domain.Entities;

public static class ProductTypeMapper
{
    public static ProductTypeDto ToDto(ProductType productType) => new(
        productType.Id.Value,
        productType.Name,
        productType.UnitPrice.Amount.HasValue ? productType.UnitPrice.Amount.Value : null);
}
