using Api.Application.Abstractions;
using Api.Domain.ValueObjects;

namespace Api.Application.Orders.Commands.Handlers;

public class PlaceOrderWithDeferredPaymentHandler
    : PlaceOrderHandlerBase<PlaceOrderWithDeferredPaymentCommand>
{
    public PlaceOrderWithDeferredPaymentHandler(
        ICustomerRepository customerRepo,
        IBatchRepository batchRepo,
        IUnitOfWork unitOfWork)
        : base(customerRepo, batchRepo, unitOfWork)
    {
    }

    protected override Money CalculateOrderTotal(PlaceOrderWithDeferredPaymentCommand command)
        => new(command.Amount);

    protected override DateTime? GetDueDate(PlaceOrderWithDeferredPaymentCommand command)
        => command.DueDate;
}
