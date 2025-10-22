using Api.Application.Dtos;
using Api.Application.Interfaces;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductTypeRepository _productTypeRepository;
    private readonly IOrderPaymentService _orderPaymentService;

    public OrderService(
        IUserRepository userRepository,
        IProductTypeRepository productTypeRepository,
        IOrderPaymentService orderPaymentService)
    {
        _userRepository = userRepository;
        _productTypeRepository = productTypeRepository;
        _orderPaymentService = orderPaymentService;
    }

    public PlaceOrderResponse PlaceOrder(UserId userId, PlaceOrderRequest request)
    {
        var user = _userRepository.GetById(userId) ?? throw new KeyNotFoundException("User not found.");
        var batch = request.BatchNumber;

        var productType = _productTypeRepository.GetById(request.ProductTypeId)
            ?? throw new KeyNotFoundException("Product type not found.");

        var charge = new Money(productType.UnitPrice);

        var order = user.PlaceOrder(userId, batch, request.ProductTypeId, new OrderDetail(charge, request.OrderDate, request.DueDate));
        _userRepository.Save(user);

        var orderDto = ToOrderDto(order);

        return new PlaceOrderResponse(orderDto.Id, orderDto.OrderDate);
    }

    public PlaceOrderResponse PlaceOrderWithPayment(UserId userId, PlaceOrderRequest request, decimal? amountPaid)
    {
        var user = _userRepository.GetById(userId) ?? throw new KeyNotFoundException("User not found.");
        var batch = request.BatchNumber;

        var productType = _productTypeRepository.GetById(request.ProductTypeId)
            ?? throw new KeyNotFoundException("Product type not found.");

        var charge = new Money(productType.UnitPrice);

        var placeOrderResponse = _orderPaymentService.PlaceOrderWithPayment(userId, request, amountPaid);
        _userRepository.Save(user);

        return new PlaceOrderResponse(placeOrderResponse.Order.OrderId, placeOrderResponse.Order.OrderDate);
    }

    private static OrderDto ToOrderDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.BatchNumber,
            order.ProductTypeId,
            order.OrderDetail.OrderDate,
            order.OrderDetail.Total.Amount
        );
    }
}