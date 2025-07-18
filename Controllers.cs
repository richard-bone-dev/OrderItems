namespace Api.Controllers;

using Api.Application;
using Api.Domain;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users/{userId:int}")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers()
        => Ok(_userService.ListUsers());

    [HttpGet("statement")]
    public ActionResult<UserStatementResponse> GetStatement(int userId)
        => Ok(_userService.GetStatement(userId));

    [HttpPost("orders")]
    public ActionResult<PlaceOrderResponse> PlaceOrder(int userId, [FromBody] PlaceOrderRequest request)
    {
        var result = _userService.PlaceOrder(userId, request);
        return CreatedAtAction(nameof(GetStatement), new { userId }, result);
    }

    [HttpPost("payments")]
    public ActionResult<MakePaymentResponse> MakePayment(int userId, [FromBody] MakePaymentRequest request)
    {
        var result = _userService.MakePayment(userId, request);
        return Created("", result);
    }
}

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