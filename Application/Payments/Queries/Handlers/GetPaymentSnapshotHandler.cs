using Api.Application.Abstractions;
using Api.Application.Orders.Commands.Handlers;
using Api.Application.Payments.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Payments.Queries.Handlers;

public class GetPaymentSnapshotHandler
    : IQueryHandlerAsync<GetPaymentSnapshotQuery, PaymentDto?>
{
    private readonly IPaymentRepository _repo;

    public GetPaymentSnapshotHandler(IPaymentRepository repo) => _repo = repo;

    public async Task<PaymentDto?> HandleAsync(GetPaymentSnapshotQuery query, CancellationToken ct = default)
    {
        var payment = await _repo.GetByIdAsync(new PaymentId(query.PaymentId), ct);
        return payment is null ? null : PaymentMapper.ToDto(payment);
    }
}