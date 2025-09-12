using Api.Application.Dtos;
using Api.Application.Interfaces;
using Api.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IOrderPaymentService _orderPaymentService;

    public UsersController(
        IUserService userService,
        IOrderService orderService,
        IPaymentService paymentService,
        IOrderPaymentService orderPaymentService)
    {
        _userService = userService;
        _orderService = orderService;
        _paymentService = paymentService;
        _orderPaymentService = orderPaymentService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers()
        => Ok(_userService.ListUsers());

    [HttpPost]
    public ActionResult<UserDto> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = _userService.CreateUser(request);
        return CreatedAtAction(nameof(GetUsers), new { userId = user.UserId }, user);
    }

    [HttpGet("{userId}/statement")]
    public ActionResult<UserStatementResponse> GetStatement(UserId userId)
        => Ok(_userService.GetStatement(userId));

    [HttpPost("{userId}/orders")]
    public ActionResult<PlaceOrderResponse> PlaceOrder(UserId userId, [FromBody] PlaceOrderRequest request)
        => Ok(_orderService.PlaceOrder(userId, request));

    [HttpPost("{userId}/order-with-payment")]
    public ActionResult<object> PlaceOrderWithPayment(UserId userId, [FromBody] PlaceOrderWithPaymentRequest request)
    {
        var result = _orderPaymentService.PlaceOrderWithPayment(userId, request.Order, request.PaymentAmount);
        return Ok(result);
    }

    [HttpPost("{userId}/payments")]
    public ActionResult<MakePaymentResponse> MakePayment(UserId userId, [FromBody] MakePaymentRequest request)
        => Ok(_paymentService.MakePayment(userId, request));

    [HttpGet("{userId}/payments")]
    public ActionResult<IEnumerable<PaymentDto>> GetUserPayments(UserId userId)
        => Ok(_paymentService.GetUserPayments(userId));
}
