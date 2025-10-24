using Api.Application.Orders.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Interfaces;

public interface IOrderService
{
    PlaceOrderResponse PlaceOrder(UserId userId, PlaceOrderRequest request);
}