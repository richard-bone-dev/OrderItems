using Api.Application.Abstractions;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _db;
    public PaymentRepository(ApplicationDbContext db) => _db = db;


    public async Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken ct = default)
    => await _db.Payments.FirstOrDefaultAsync(p => p.Id.Value == id.Value, ct);


    public async Task<IReadOnlyCollection<Payment>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken ct = default)
    => await _db.Payments.Where(p => p.CustomerId == customerId).ToListAsync(ct);


    public async Task<IReadOnlyCollection<Payment>> GetAllAsync(CancellationToken ct = default)
    => await _db.Payments.ToListAsync(ct);


    public async Task AddAsync(Payment payment, CancellationToken ct = default)
    => await _db.Payments.AddAsync(payment, ct);
}