using Api.Application.Orders.Dtos;
using Api.Application.Payments.Dtos;
using Api.Application.Interfaces;
using Api.Domain.Events;
using Api.Domain.ValueObjects;

namespace Api.Application.Services;

public class OrderPaymentService : IOrderPaymentService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductTypeRepository _productTypeRepository;
    private readonly IBatchAssignmentService _batchService;

    public OrderPaymentService(
        IUserRepository userRepository,
        IProductTypeRepository productTypeRepository,
        IBatchAssignmentService batchService)
    {
        _userRepository = userRepository;
        _productTypeRepository = productTypeRepository;
        _batchService = batchService;
    }

    public (PlaceOrderResponse Order, MakePaymentResponse? Payment) PlaceOrderWithPayment(
        UserId userId,
        PlaceOrderRequest request,
        decimal? paymentAmount = null)
    {
        var user = _userRepository.GetById(userId) ?? throw new KeyNotFoundException("User not found.");
        var batch = request.BatchNumber ?? _batchService.GetCurrentBatch();

        var productType = _productTypeRepository.GetById(request.ProductTypeId)
            ?? throw new KeyNotFoundException("Product type not found.");

        var charge = new Money(productType.UnitPrice);

        // Place order
        var order = user.PlaceOrder(userId, batch, request.ProductTypeId, new OrderDetail(charge, request.OrderDate, request.DueDate));

        // Payment handling
        MakePaymentResponse? paymentResponse = null;
        if (paymentAmount.HasValue)
        {
            var paid = new Money(paymentAmount.Value);
            var remaining = new Money(charge.Amount - paymentAmount.Value);

            var payment = user.MakePayment(userId, paid, remaining, request.DueDate);
            paymentResponse = new MakePaymentResponse(payment.Id, payment.PaidAmount.Amount, payment.PaymentDate);
        }

        _userRepository.Save(user);

        var orderResponse = new PlaceOrderResponse(order.Id, order.OrderDetail.OrderDate);
        return (orderResponse, paymentResponse);
    }
}