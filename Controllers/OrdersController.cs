using Api.Application.Orders.Commands;
using Api.Application.Orders.Commands.Handlers;
using Api.Application.Orders.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly PlaceOrderWithImmediatePaymentHandler _immediateHandler;
    private readonly PlaceOrderWithDeferredPaymentHandler _deferredHandler;
    private readonly PlaceOrderWithPartialPaymentHandler _partialHandler;

    public OrdersController(
        PlaceOrderWithImmediatePaymentHandler immediateHandler,
        PlaceOrderWithDeferredPaymentHandler deferredHandler,
        PlaceOrderWithPartialPaymentHandler partialHandler)
    {
        _immediateHandler = immediateHandler;
        _deferredHandler = deferredHandler;
        _partialHandler = partialHandler;
    }

    [HttpPost("immediate")]
    public async Task<ActionResult<OrderDto>> PlaceImmediate([FromBody] PlaceOrderWithImmediatePaymentCommand cmd, CancellationToken ct)
        => Ok(await _immediateHandler.Handle(cmd, ct));

    [HttpPost("deferred")]
    public async Task<ActionResult<OrderDto>> PlaceDeferred([FromBody] PlaceOrderWithDeferredPaymentCommand cmd, CancellationToken ct)
        => Ok(await _deferredHandler.Handle(cmd, ct));

    [HttpPost("partial")]
    public async Task<ActionResult<OrderDto>> PlacePartial([FromBody] PlaceOrderWithPartialPaymentCommand cmd, CancellationToken ct)
        => Ok(await _partialHandler.Handle(cmd, ct));
}