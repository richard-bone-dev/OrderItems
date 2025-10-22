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
    private readonly GetUserPaymentsHandler _getUserPayments;
    private readonly GetPaymentDetailHandler _getDetail;
    private readonly GetPaymentSnapshotHandler _getSnapshot;

    public PaymentsController(
        RecordPaymentHandler recordHandler,
        GetUserPaymentsHandler getUserPayments,
        GetPaymentDetailHandler getDetail,
        GetPaymentSnapshotHandler getSnapshot)
    {
        _recordHandler = recordHandler;
        _getUserPayments = getUserPayments;
        _getDetail = getDetail;
        _getSnapshot = getSnapshot;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Record([FromBody] RecordPaymentCommand cmd, CancellationToken ct)
        => Ok(await _recordHandler.Handle(cmd, ct));

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUserPayments(Guid userId, CancellationToken ct)
        => Ok(await _getUserPayments.Handle(new GetUserPaymentsQuery(userId), ct));

    [HttpGet("{paymentId:guid}")]
    public async Task<ActionResult<PaymentDetailDto>> GetDetail(Guid paymentId, CancellationToken ct)
        => Ok(await _getDetail.Handle(new GetPaymentDetailQuery(paymentId), ct));

    [HttpGet("{paymentId:guid}/snapshot")]
    public async Task<ActionResult<PaymentSnapshotDto>> GetSnapshot(Guid paymentId, CancellationToken ct)
        => Ok(await _getSnapshot.Handle(new GetPaymentSnapshotQuery(paymentId), ct));
}