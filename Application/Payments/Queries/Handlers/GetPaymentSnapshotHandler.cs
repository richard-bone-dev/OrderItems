using Api.Application.Abstractions;
using Api.Application.Payments.Dtos;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Payments.Queries.Handlers;

public class GetPaymentSnapshotHandler
    : IQueryHandler<GetPaymentSnapshotQuery, PaymentSnapshotDto?>
{
    private readonly ApplicationDbContext _db;

    public GetPaymentSnapshotHandler(ApplicationDbContext db) => _db = db;

    public async Task<PaymentSnapshotDto?> Handle(GetPaymentSnapshotQuery query, CancellationToken ct = default)
    {
        return await _db.Payments
            .Where(p => p.Id == new Domain.ValueObjects.PaymentId(query.PaymentId))
            .Select(p => new PaymentSnapshotDto(
                p.Id.Value,
                p.UserId.Value,
                p.PaidAmount.Amount,
                p.PaymentDate
            ))
            .FirstOrDefaultAsync(ct);
    }
}
