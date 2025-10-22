using Api.Application.Abstractions;
using Api.Application.Dtos;
using Api.Application.Orders.Commands.Handlers;
using Api.Application.Customers.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Customers.Queries.Handlers;

public class GetCustomerStatementHandler : IQueryHandlerAsync<GetCustomerStatementQuery, CustomerStatementResponse>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IBatchRepository _batchRepository;


    public GetCustomerStatementHandler(ICustomerRepository customerRepository, IOrderRepository orderRepository, IBatchRepository batchRepository)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _batchRepository = batchRepository;
    }


    public async Task<CustomerStatementResponse> HandleAsync(GetCustomerStatementQuery query, CancellationToken ct)
    {
        var userId = new CustomerId(query.CustomerId);


        var user = await _customerRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");


        var orders = await _orderRepository.GetByCustomerIdAsync(userId, ct);


        var batchIds = orders.Select(o => o.BatchId).Distinct().ToList();
        var batchMap = new Dictionary<BatchId, BatchNumber>();

        foreach (var batchId in batchIds)
        {
            var batch = await _batchRepository.GetByIdAsync(batchId, ct);
            if (batch is not null)
                batchMap[batchId] = batch.Number;
        }


        var orderDtos = orders.Select(order => order.ToDto(batchMap)).ToList();


        var paymentDtos = user.Payments.Select(PaymentMapper.ToDto).ToList();


        return new CustomerStatementResponse(
            user.Id.Value,
            user.Name.Value,
            user.TotalCharged.Amount,
            user.TotalPaid.Amount,
            user.Balance.Amount,
            orderDtos,
            paymentDtos
        );
    }
}