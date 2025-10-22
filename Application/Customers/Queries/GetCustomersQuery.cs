using Api.Application.Abstractions;
using Api.Application.Customers.Dtos;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Customers.Queries;

public record GetCustomersQuery();
public record GetCustomerStatementQuery(CustomerId CustomerId);

public class GetCustomersHandler : IQueryHandlerAsync<GetCustomersQuery, IEnumerable<CustomerDto>>
{
    private readonly ApplicationDbContext _db;

    public GetCustomersHandler(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<CustomerDto>> HandleAsync(GetCustomersQuery query, CancellationToken ct = default)
    {
        return await _db.Customers
            .Select(u => new CustomerDto(u.Id.Value, u.Name.Value, u.RegisteredAt))
            .ToListAsync(ct);
    }
}