using Api.Application.Abstractions;
using Api.Application.Orders.Dtos;
using Api.Application.Payments.Dtos;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Orders.Commands.Handlers;

public class PlaceOrderWithPartialPaymentHandler
    : ICommandHandler<PlaceOrderWithPartialPaymentCommand, OrderDto>
{
    private readonly IUserRepository _userRepo;
    private readonly IBatchRepository _batchRepo;

    public PlaceOrderWithPartialPaymentHandler(IUserRepository userRepo, IBatchRepository batchRepo)
    {
        _userRepo = userRepo;
        _batchRepo = batchRepo;
    }

    public async Task<OrderDto> Handle(PlaceOrderWithPartialPaymentCommand cmd, CancellationToken ct = default)
    {
        var userId = new UserId(cmd.UserId);
        var batchId = new BatchId(cmd.BatchId);
        var productTypeId = new ProductTypeId(cmd.ProductTypeId);

        var user = await _userRepo.GetByIdAsync(userId, ct)
                   ?? throw new KeyNotFoundException("User not found.");
        var batch = await _batchRepo.GetByIdAsync(batchId, ct)
                    ?? throw new KeyNotFoundException("Batch not found.");

        var total = new Money(cmd.PaidAmount + cmd.RemainingAmount);

        var order = batch.AddOrder(userId, productTypeId, total, DateTime.UtcNow, cmd.DueDate);

        var payment = Payment.Create(userId, cmd.PaidAmount, DateTime.UtcNow);
        user.AddPayment(payment);

        await _batchRepo.SaveChangesAsync(ct);
        await _userRepo.SaveChangesAsync(ct);

        return OrderMapper.ToDto(order, batch.Number);
    }
}

public static class PaymentMapper
{
    public static PaymentDto ToDto(Payment payment) => new(
        payment.Id.Value,
        payment.UserId.Value,
        payment.PaidAmount.Amount,
        payment.PaymentDate
    );
}

public static class OrderMapper
{
    public static OrderDto ToDto(Order order, BatchNumber batchNumber) => new(
        order.Id.Value,
        order.UserId.Value,
        order.BatchId.Value,
        batchNumber.Value,
        order.OrderDetail.ProductTypeId.Value,
        order.OrderDetail.UnitPrice.Amount,
        order.OrderDetail.Quantity,
        order.OrderDetail.Total.Amount,
        order.OrderDetail.PlacedAt,
        order.OrderDetail.DueDate
    );
}