using Api.Application.Abstractions;
using Api.Application.ProductTypes.Dtos;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;

namespace Api.Application.ProductTypes.Commands.Handlers;

public class CreateProductTypeHandler
    : ICommandHandler<CreateProductTypeCommand, ProductTypeDto>
{
    private readonly ApplicationDbContext _db;

    public CreateProductTypeHandler(ApplicationDbContext db) => _db = db;

    public async Task<ProductTypeDto> Handle(CreateProductTypeCommand cmd, CancellationToken ct = default)
    {
        var productType = ProductType.Create(cmd.Name, new Money(cmd.UnitPrice));

        await _db.ProductTypes.AddAsync(productType, ct);
        await _db.SaveChangesAsync(ct);

        return new ProductTypeDto(
            productType.Id.Value,
            productType.Name,
            productType.UnitPrice.Amount
        );
    }
}
