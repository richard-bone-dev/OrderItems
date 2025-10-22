using Api.Application.Abstractions;
using Api.Application.Orders.Commands.Handlers;
using Api.Application.Payments.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Payments.Queries.Handlers;

public class GetCustomerPaymentsHandler
    : IQueryHandlerAsync<GetCustomerPaymentsQuery, IEnumerable<PaymentDto>>
{
    private readonly IPaymentRepository _repo;
    public GetCustomerPaymentsHandler(IPaymentRepository repo) => _repo = repo;

    public async Task<IEnumerable<PaymentDto>> HandleAsync(GetCustomerPaymentsQuery query, CancellationToken ct = default)
    {
        var payments = await _repo.GetByCustomerIdAsync(new CustomerId(query.CustomerId), ct);
        return payments is null ? null : payments.Select(PaymentMapper.ToDto);
    }
}