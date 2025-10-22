namespace Api.Application.Orders.Commands;

public record PlaceOrderWithImmediatePaymentCommand(
    Guid CustomerId,
    Guid BatchId,
    Guid ProductTypeId,
    decimal Amount);
