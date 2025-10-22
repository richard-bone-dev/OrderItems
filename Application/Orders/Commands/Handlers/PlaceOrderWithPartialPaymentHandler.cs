using Api.Application.Abstractions;
using Api.Application.Orders.Dtos;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Orders.Commands.Handlers;

public class PlaceOrderWithPartialPaymentHandler
    : ICommandHandlerAsync<PlaceOrderWithPartialPaymentCommand, OrderDto>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IBatchRepository _batchRepo;

    public PlaceOrderWithPartialPaymentHandler(ICustomerRepository customerRepo, IBatchRepository batchRepo)
    {
        _customerRepo = customerRepo;
        _batchRepo = batchRepo;
    }

    public async Task<OrderDto> HandleAsync(PlaceOrderWithPartialPaymentCommand cmd, CancellationToken ct = default)
    {
        var customerId = new CustomerId(cmd.CustomerId);
        var batchId = new BatchId(cmd.BatchId);
        var productTypeId = new ProductTypeId(cmd.ProductTypeId);

        var customer = await _customerRepo.GetByIdAsync(customerId, ct)
                   ?? throw new KeyNotFoundException("User not found.");
        var batch = await _batchRepo.GetByIdAsync(batchId, ct)
                    ?? throw new KeyNotFoundException("Batch not found.");

        var total = new Money(cmd.PaidAmount + cmd.RemainingAmount);

        var order = batch.AddOrder(customerId, productTypeId, total, DateTime.UtcNow, cmd.DueDate);

        var payment = Payment.Create(customerId, cmd.PaidAmount, DateTime.UtcNow);
        customer.AddPayment(payment);

        await _batchRepo.SaveChangesAsync(ct);
        await _customerRepo.SaveChangesAsync(ct);

        return OrderMapper.ToDto(order, batch.Number);
    }
