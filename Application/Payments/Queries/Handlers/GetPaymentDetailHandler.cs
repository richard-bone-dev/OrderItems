using Api.Application.Abstractions;
using Api.Application.Payments.Dtos;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Payments.Queries.Handlers;

public class GetPaymentDetailHandler
    : IQueryHandler<GetPaymentDetailQuery, PaymentDetailDto?>
{
    private readonly ApplicationDbContext _db;

    public GetPaymentDetailHandler(ApplicationDbContext db) => _db = db;

    public async Task<PaymentDetailDto?> Handle(GetPaymentDetailQuery query, CancellationToken ct = default)
    {
        return await _db.Payments
            .Where(p => p.Id == new Domain.ValueObjects.PaymentId(query.PaymentId))
            .Select(p => new PaymentDetailDto(
                p.Id.Value,
                p.UserId.Value,
                p.PaidAmount.Amount,
                p.PaymentDate
            ))
            .FirstOrDefaultAsync(ct);
    }
}
