using Api.Application.Interfaces;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _db;
    public PaymentRepository(ApplicationDbContext db) => _db = db;

    public Payment GetById(PaymentId paymentId)
        => _db.Payments.Single(p => p.Id == paymentId);

    public IEnumerable<Payment> GetByUserId(UserId userId)
        => _db.Payments
              .Where(p => p.UserId == userId)
              .AsNoTracking()
              .ToList();

    public void Save(Payment payment)
    {
        if (_db.Entry(payment).State == EntityState.Detached)
            _db.Payments.Add(payment);

        _db.SaveChanges();
    }
}
