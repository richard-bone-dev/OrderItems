using System;
using Api.Application.Abstractions;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Orders.Commands.Handlers;

public class PlaceOrderWithImmediatePaymentHandler
    : PlaceOrderHandlerBase<PlaceOrderWithImmediatePaymentCommand>
{
    public PlaceOrderWithImmediatePaymentHandler(ICustomerRepository customerRepo, IBatchRepository batchRepo)
        : base(customerRepo, batchRepo)
    {
    }

    protected override Money CalculateOrderTotal(PlaceOrderWithImmediatePaymentCommand command)
        => new(command.Amount);

    protected override Task<bool> HandleCustomerUpdatesAsync(
        Customer customer,
        Order _,
        PlaceOrderWithImmediatePaymentCommand command,
        CancellationToken ct)
    {
        var payment = Payment.Create(customer.Id.Value, command.Amount, DateTime.UtcNow);
        customer.AddPayment(payment);
        return Task.FromResult(true);
    }
}
