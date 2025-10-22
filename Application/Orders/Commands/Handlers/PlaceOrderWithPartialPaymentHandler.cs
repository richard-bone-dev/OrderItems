using Api.Application.Abstractions;
using Api.Application.Orders.Dtos;
using Api.Application.Payments.Dtos;
using Api.Application.ProductTypes.Dtos;
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
}

public static class ProductTypeMapper
{
    public static ProductTypeDto ToDto(ProductType productType) => new(
        productType.Id.Value,
        productType.Name,
        productType.UnitPrice.Amount.HasValue ? productType.UnitPrice.Amount.Value : null
    );
}

public static class PaymentMapper
{
    public static PaymentDto ToDto(Payment payment) => new(
        payment.Id.Value,
        payment.CustomerId.Value,
        payment.PaidAmount.Amount,
        payment.PaymentDate
    );
}

public static class OrderMapper
{
    public static OrderDto ToDto(Order order, BatchNumber batchNumber) => new(
        order.Id.Value,
        order.CustomerId.Value,
        order.BatchId.Value,
        batchNumber.Value,
        order.OrderDetails.First().ProductTypeId.Value,
        order.OrderDetails.First().UnitPrice.Amount,
        order.OrderDetails.First().Quantity,
        order.OrderDetails.First().Total.Amount,
        order.OrderDetails.First().PlacedAt,
        order.OrderDetails.First().DueDate
    );

    public static OrderDto ToDto(this Order order, Dictionary<BatchId, BatchNumber> batchMap)
    {
        var batchNumber = batchMap.TryGetValue(order.BatchId, out var bn)
            ? bn.Value
            : 0;

        return new OrderDto(
            order.Id.Value,
            order.CustomerId.Value,
            order.BatchId.Value,
            batchNumber,
            order.OrderDetails.First().ProductTypeId.Value,
            order.OrderDetails.First().UnitPrice.Amount,
            order.OrderDetails.First().Quantity,
            order.OrderDetails.First().Total.Amount,
            order.OrderDetails.First().PlacedAt,
            order.OrderDetails.First().DueDate
        );
    }
}