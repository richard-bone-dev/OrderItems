namespace Api.Application.Orders.Commands;

public record PlaceOrderWithPartialPaymentCommand(
    Guid UserId,
    Guid BatchId,
    Guid ProductTypeId,
    decimal PaidAmount,
    decimal RemainingAmount,
    DateTime? DueDate
);
