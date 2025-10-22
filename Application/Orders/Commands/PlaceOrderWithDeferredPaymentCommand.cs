namespace Api.Application.Orders.Commands;

public record PlaceOrderWithDeferredPaymentCommand(
    Guid CustomerId,
    Guid BatchId,
    Guid ProductTypeId,
    decimal Amount,
    DateTime DueDate);
