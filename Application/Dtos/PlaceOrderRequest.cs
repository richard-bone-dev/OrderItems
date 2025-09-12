using Api.Domain.ValueObjects;

namespace Api.Application.Dtos;

public record PlaceOrderRequest(BatchNumber BatchNumber, ProductTypeId ProductTypeId, DateTime OrderDate, DateTime DueDate, bool Settled = false);
public record PlaceOrderResponse(OrderId OrderId, DateTime? OrderDate);

public record PaymentDto(Guid Id, decimal Amount, DateTime PaymentDate);
public record MakePaymentRequest(decimal Amount, DateTime? PaymentDate = null);
public record MakePaymentResponse(Guid PaymentId, decimal Amount, DateTime PaymentDate);

public record UserDto(UserId UserId, string Name, bool Preferred, decimal? Balance);
public class CreateUserRequest { public string Name { get; set; } = string.Empty; }
public record UserStatementResponse(string Name, decimal TotalCharged, decimal TotalPaid, decimal Balance,
    IEnumerable<OrderDto> Orders, IEnumerable<PaymentDto> Payments);

public record ProductTypeDto(Guid Id, decimal? UnitPrice);

public record OrderDto(OrderId Id, BatchNumber BatchNumber, ProductTypeId ProductTypeId, DateTime? OrderDate, decimal Amount);

public record PlaceOrderWithPaymentRequest(PlaceOrderRequest Order, decimal? PaymentAmount);

public record OrderWithPaymentResponse(PlaceOrderResponse Order, MakePaymentResponse? Payment);