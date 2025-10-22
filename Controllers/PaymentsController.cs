using Api.Application.Payments.Commands;
using Api.Application.Payments.Dtos;
using Api.Application.Payments.Queries;
using Api.Application.Payments.Queries.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly RecordPaymentHandler _recordHandler;
    private readonly GetCustomerPaymentsHandler _getCustomerPayments;
    private readonly GetPaymentDetailHandler _getDetail;
    private readonly GetPaymentSnapshotHandler _getSnapshot;

    public PaymentsController(
        RecordPaymentHandler recordHandler,
        GetCustomerPaymentsHandler getCustomerPayments,
        GetPaymentDetailHandler getDetail,
        GetPaymentSnapshotHandler getSnapshot)
    {
        _recordHandler = recordHandler;
        _getCustomerPayments = getCustomerPayments;
        _getDetail = getDetail;
        _getSnapshot = getSnapshot;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> RecordAsync([FromBody] RecordPaymentCommand cmd, CancellationToken ct)
        => Ok(await _recordHandler.HandleAsync(cmd, ct));

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetCustomerPaymentsAsync(Guid customerId, CancellationToken ct)
        => Ok(await _getCustomerPayments.HandleAsync(new GetCustomerPaymentsQuery(customerId), ct));

    [HttpGet("{paymentId:guid}")]
    public async Task<ActionResult<PaymentDetailDto>> GetDetailAsync(Guid paymentId, CancellationToken ct)
        => Ok(await _getDetail.HandleAsync(new GetPaymentDetailQuery(paymentId), ct));

    [HttpGet("{paymentId:guid}/snapshot")]
    public async Task<ActionResult<PaymentSnapshotDto>> GetSnapshotAsync(Guid paymentId, CancellationToken ct)
        => Ok(await _getSnapshot.HandleAsync(new GetPaymentSnapshotQuery(paymentId), ct));
}