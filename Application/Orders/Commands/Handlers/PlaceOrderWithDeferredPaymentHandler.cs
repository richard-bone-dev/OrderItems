using Api.Application.Abstractions;
using Api.Application.Orders.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Orders.Commands.Handlers;

public class PlaceOrderWithDeferredPaymentHandler
    : ICommandHandler<PlaceOrderWithDeferredPaymentCommand, OrderDto>
{
    private readonly IUserRepository _userRepo;
    private readonly IBatchRepository _batchRepo;

    public PlaceOrderWithDeferredPaymentHandler(IUserRepository userRepo, IBatchRepository batchRepo)
    {
        _userRepo = userRepo;
        _batchRepo = batchRepo;
    }

    public async Task<OrderDto> Handle(PlaceOrderWithDeferredPaymentCommand cmd, CancellationToken ct = default)
    {
        var userId = new UserId(cmd.UserId);
        var batchId = new BatchId(cmd.BatchId);
        var productTypeId = new ProductTypeId(cmd.ProductTypeId);

        var user = await _userRepo.GetByIdAsync(userId, ct)
                   ?? throw new KeyNotFoundException("User not found.");
        var batch = await _batchRepo.GetByIdAsync(batchId, ct)
                    ?? throw new KeyNotFoundException("Batch not found.");

        var total = new Money(cmd.Amount);

        // Add order but no payment yet
        var order = batch.AddOrder(userId, productTypeId, total, DateTime.UtcNow, cmd.DueDate);

        await _batchRepo.SaveChangesAsync(ct);

        return OrderMapper.ToDto(order, batch.Number);
    }
}