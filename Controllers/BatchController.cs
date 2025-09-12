namespace Api.Controllers;

using Api.Domain.Events;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/batch")]
public class BatchController : ControllerBase
{
    private readonly IBatchAssignmentService _batchService;

    public BatchController(IBatchAssignmentService batchService)
    {
        _batchService = batchService;
    }

    [HttpGet("current")]
    public ActionResult<int> GetCurrentBatch()
        => Ok(_batchService.GetCurrentBatch().Value);

    [HttpPost("advance")]
    public IActionResult AdvanceBatch()
    {
        _batchService.AdvanceToNextBatch();
        return NoContent();
    }
}

//[Route("api/payments")]
//public class PaymentsController : ControllerBase
//{
//    private readonly IPaymentQueries _queries;

//    public PaymentsController(IPaymentQueries queries) => _queries = queries;

//    [HttpGet("{id:guid}/snapshot")]
//    public async Task<ActionResult<PaymentSnapshotDto>> GetSnapshot(Guid id)
//    {
//        var result = await _queries.GetSnapshotAsync(id);
//        return result is null ? NotFound() : Ok(result);
//    }

//    [HttpGet("{id:guid}/detail")]
//    public async Task<ActionResult<PaymentDetailDto>> GetDetail(Guid id)
//    {
//        var result = await _queries.GetDetailAsync(id);
//        return result is null ? NotFound() : Ok(result);
//    }
//}


//[ApiController]
//[Route("api/payments")]
//public class PaymentsController : ControllerBase
//{
//    private readonly PaymentService _service;

//    public PaymentsController(PaymentService service)
//    {
//        _service = service;
//    }

//    [HttpPost]
//    public ActionResult<MakePaymentResponse> MakePayment(
//        [FromRoute] UserId userId,
//        [FromBody] MakePaymentRequest request)
//    {
//        var result = _service.MakePayment(userId, request);
//        return Ok(result);
//    }

//    [HttpGet]
//    public ActionResult<IEnumerable<PaymentDto>> GetUserPayments([FromRoute] UserId userId)
//    {
//        var payments = _service.GetUserPayments(userId);
//        return Ok(payments);
//    }
//}
