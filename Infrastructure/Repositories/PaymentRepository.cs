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

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default) 
        => _db.Payments.AddAsync(payment, ct).AsTask();

    public async Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken ct) 
        => await _db.Payments.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyCollection<Payment>> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
        => await _db.Payments.Where(b => b.UserId == userId).ToListAsync();
}
