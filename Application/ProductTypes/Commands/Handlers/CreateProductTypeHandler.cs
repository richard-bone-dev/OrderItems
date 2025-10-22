using Api.Application.Abstractions;
using Api.Application.ProductTypes.Dtos;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.ProductTypes.Commands.Handlers;

public class CreateProductTypeHandler
    : ICommandHandlerAsync<CreateProductTypeCommand, ProductTypeDto>
{
    private readonly IProductTypeRepository _repo;

    public CreateProductTypeHandler(IProductTypeRepository repo) => _repo = repo;

    public async Task<ProductTypeDto> HandleAsync(CreateProductTypeCommand cmd, CancellationToken ct = default)
    {
        var productType = ProductType.Create(cmd.Name, new Money(cmd.UnitPrice));
        await _repo.AddAsync(productType, ct);

        return new ProductTypeDto(
            productType.Id.Value,
            productType.Name,
            productType.UnitPrice.Amount
        );
    }
}