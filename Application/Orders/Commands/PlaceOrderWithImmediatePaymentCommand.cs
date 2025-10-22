namespace Api.Application.Orders.Commands;

public record PlaceOrderWithImmediatePaymentCommand(
    Guid UserId,
    Guid BatchId,
    Guid ProductTypeId,
    decimal Amount);
