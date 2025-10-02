namespace Api.Application.Orders.Commands;

public record PlaceOrderWithDeferredPaymentCommand(
    Guid UserId,
    Guid BatchId,
    Guid ProductTypeId,
    decimal Amount,
    DateTime DueDate);
