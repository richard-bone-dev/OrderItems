using Api.Application.Abstractions;
using Api.Application.Payments.Dtos;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Payments.Queries.Handlers;

public class GetUserPaymentsHandler
    : IQueryHandler<GetUserPaymentsQuery, IEnumerable<PaymentDto>>
{
    private readonly ApplicationDbContext _db;

    public GetUserPaymentsHandler(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<PaymentDto>> Handle(GetUserPaymentsQuery query, CancellationToken ct = default)
    {
        return await _db.Payments
            .Where(p => p.UserId == new Domain.ValueObjects.UserId(query.UserId))
            .Select(p => new PaymentDto(
                p.Id.Value,
                p.UserId.Value,
                p.PaidAmount.Amount,
                p.PaymentDate
            ))
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(ct);
    }
}