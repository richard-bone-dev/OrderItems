namespace Api.Application.Orders.Commands;

public interface IPlaceOrderCommand
{
    Guid CustomerId { get; }
    Guid BatchId { get; }
    Guid ProductTypeId { get; }
}
