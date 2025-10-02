using Api.Application.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Interfaces;

public interface IOrderService
{
    PlaceOrderResponse PlaceOrder(UserId userId, PlaceOrderRequest request);
}