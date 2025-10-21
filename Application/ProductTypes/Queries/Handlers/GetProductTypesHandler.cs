using Api.Application.Abstractions;
using Api.Application.Orders.Commands.Handlers;
using Api.Application.ProductTypes.Dtos;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.ProductTypes.Queries.Handlers;

public class GetProductTypesHandler
    : IQueryHandler<GetProductTypeQuery, IEnumerable<ProductTypeDto>?>
{
    private readonly ApplicationDbContext _db;

    public GetProductTypesHandler(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<ProductTypeDto>?> Handle(GetProductTypeQuery query, CancellationToken ct = default)
    {
        var productTypes = await _db.ProductTypes
            .OrderBy(p => p.Name)
            .ToListAsync();

        return productTypes is null ? null : productTypes.Select(ProductTypeMapper.ToDto);
    }
}