using Api.Application.Abstractions;
using Api.Application.Orders.Commands.Handlers;
using Api.Application.Payments.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Payments.Queries.Handlers;

public class GetUserPaymentsHandler
    : IQueryHandler<GetUserPaymentsQuery, IEnumerable<PaymentDto>>
{
    private readonly IPaymentRepository _repo;
    public GetUserPaymentsHandler(IPaymentRepository repo) => _repo = repo;

    public async Task<IEnumerable<PaymentDto>> Handle(GetUserPaymentsQuery query, CancellationToken ct = default)
    {
        var payments = await _repo.GetByUserIdAsync(new CustomerId(query.UserId), ct);
        return payments is null ? null : payments.Select(PaymentMapper.ToDto);
    }
}