using Api.Application.Abstractions;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _db;
    public CustomerRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
        => await _db.Customers.AddAsync(customer, ct);

    public async Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken ct = default)
        => await _db.Customers.Include(u => u.Orders).Include(u => u.Payments).FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken ct = default)
        => await _db.Customers.Include(u => u.Orders).Include(u => u.Payments).ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}