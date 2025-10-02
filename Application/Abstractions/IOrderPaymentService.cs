using Api.Application.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Interfaces;

public interface IOrderPaymentService
{
    (PlaceOrderResponse Order, MakePaymentResponse? Payment) PlaceOrderWithPayment(
        UserId userId,
        PlaceOrderRequest request,
        decimal? paymentAmount = null
    );
}