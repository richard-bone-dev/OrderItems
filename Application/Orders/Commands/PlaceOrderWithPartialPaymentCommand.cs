namespace Api.Application.Orders.Commands;

public record PlaceOrderWithPartialPaymentCommand(
    Guid CustomerId,
    Guid BatchId,
    Guid ProductTypeId,
    decimal PaidAmount,
    decimal RemainingAmount,
    DateTime? DueDate
);
