using Api.Application.Abstractions;
using Api.Application.Payments.Commands;
using Api.Application.Payments.Dtos;
using Api.Application.Payments.Exceptions;
using Api.Application.Payments.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ICommandHandlerAsync<RecordPaymentCommand, PaymentDto> _recordHandler;
    private readonly IQueryHandlerAsync<GetCustomerPaymentsQuery, IEnumerable<PaymentDto>?> _getCustomerPayments;
    private readonly IQueryHandlerAsync<GetPaymentDetailQuery, PaymentDto?> _getDetail;
    private readonly IQueryHandlerAsync<GetPaymentSnapshotQuery, PaymentDto?> _getSnapshot;

    public PaymentsController(
        ICommandHandlerAsync<RecordPaymentCommand, PaymentDto> recordHandler,
        IQueryHandlerAsync<GetCustomerPaymentsQuery, IEnumerable<PaymentDto>?> getCustomerPayments,
        IQueryHandlerAsync<GetPaymentDetailQuery, PaymentDto?> getDetail,
        IQueryHandlerAsync<GetPaymentSnapshotQuery, PaymentDto?> getSnapshot)
    {
        _recordHandler = recordHandler;
        _getCustomerPayments = getCustomerPayments;
        _getDetail = getDetail;
        _getSnapshot = getSnapshot;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> RecordAsync([FromBody] RecordPaymentCommand? cmd, CancellationToken ct)
    {
        if (cmd is null)
        {
            return BadRequest(new { error = "Request body cannot be null." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _recordHandler.HandleAsync(cmd, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (PaymentDeclinedException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
        }
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetCustomerPaymentsAsync(Guid customerId, CancellationToken ct)
    {
        var result = await _getCustomerPayments.HandleAsync(new GetCustomerPaymentsQuery(customerId), ct);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{paymentId:guid}")]
    public async Task<ActionResult<PaymentDto>> GetDetailAsync(Guid paymentId, CancellationToken ct)
    {
        var result = await _getDetail.HandleAsync(new GetPaymentDetailQuery(paymentId), ct);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{paymentId:guid}/snapshot")]
    public async Task<ActionResult<PaymentDto>> GetSnapshotAsync(Guid paymentId, CancellationToken ct)
    {
        var result = await _getSnapshot.HandleAsync(new GetPaymentSnapshotQuery(paymentId), ct);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}