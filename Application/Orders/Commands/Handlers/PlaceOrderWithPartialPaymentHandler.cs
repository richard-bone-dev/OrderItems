using System;
using Api.Application.Abstractions;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Orders.Commands.Handlers;

public class PlaceOrderWithPartialPaymentHandler
    : PlaceOrderHandlerBase<PlaceOrderWithPartialPaymentCommand>
{
    public PlaceOrderWithPartialPaymentHandler(ICustomerRepository customerRepo, IBatchRepository batchRepo)
        : base(customerRepo, batchRepo)
    {
    }

    protected override Money CalculateOrderTotal(PlaceOrderWithPartialPaymentCommand command)
        => new(command.PaidAmount + command.RemainingAmount);

    protected override DateTime? GetDueDate(PlaceOrderWithPartialPaymentCommand command)
        => command.DueDate;

    protected override Task<bool> HandleCustomerUpdatesAsync(
        Customer customer,
        Order _,
        PlaceOrderWithPartialPaymentCommand command,
        CancellationToken ct)
    {
        var payment = Payment.Create(customer.Id.Value, command.PaidAmount, DateTime.UtcNow);
        customer.AddPayment(payment);
        return Task.FromResult(true);
    }
}
