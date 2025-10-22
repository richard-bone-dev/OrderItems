using Api.Application.Abstractions;
using Api.Application.Orders.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Orders.Commands.Handlers;

public class PlaceOrderWithDeferredPaymentHandler
    : ICommandHandlerAsync<PlaceOrderWithDeferredPaymentCommand, OrderDto>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IBatchRepository _batchRepo;

    public PlaceOrderWithDeferredPaymentHandler(ICustomerRepository customerRepo, IBatchRepository batchRepo)
    {
        _customerRepo = customerRepo;
        _batchRepo = batchRepo;
    }

    public async Task<OrderDto> HandleAsync(PlaceOrderWithDeferredPaymentCommand cmd, CancellationToken ct = default)
    {
        var customerId = new CustomerId(cmd.CustomerId);
        var batchId = new BatchId(cmd.BatchId);
        var productTypeId = new ProductTypeId(cmd.ProductTypeId);

        var customer = await _customerRepo.GetByIdAsync(customerId, ct)
                   ?? throw new KeyNotFoundException("User not found.");
        var batch = await _batchRepo.GetByIdAsync(batchId, ct)
                    ?? throw new KeyNotFoundException("Batch not found.");

        var total = new Money(cmd.Amount);

        // Add order but no payment yet
        var order = batch.AddOrder(customerId, productTypeId, total, DateTime.UtcNow, cmd.DueDate);

        await _batchRepo.SaveChangesAsync(ct);

        return OrderMapper.ToDto(order, batch.Number);
    }
}