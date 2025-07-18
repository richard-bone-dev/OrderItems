using Api.Domain;

namespace Api.Application;

public record PlaceOrderRequest(int BatchNumber, decimal Quantity, decimal ChargeAmount, string Currency);
public record PlaceOrderResponse(int OrderId, DateTime PlacedAt);
public record MakePaymentRequest(decimal Amount, string Currency);
public record MakePaymentResponse(int PaymentId, DateTime Date);
public record UserStatementResponse(decimal TotalCharged, decimal TotalPaid, decimal Balance,
    IEnumerable<OrderDto> Orders, IEnumerable<PaymentDto> Payments);

public record UserDto(int Id, DateTime RegisteredAt);
public record OrderDto(int Id, DateTime PlacedAt, int BatchNumber, decimal Quantity, decimal Charge);
public record PaymentDto(int Id, DateTime Date, decimal Amount);

// Repository Interfaces (wrapped domain repositories)
public interface IUserRepository
{
    User GetById(int userId);
    IEnumerable<User> GetAll();
    void Save(User user);
}

// Application Service
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBatchAssignmentService _batchService;

    public UserService(IUserRepository userRepository, IBatchAssignmentService batchService)
    {
        _userRepository = userRepository;
        _batchService = batchService;
    }

    // New method to list users
    public IEnumerable<UserDto> ListUsers()
    {
        return _userRepository.GetAll()
            .Select(u => new UserDto(u.Id, u.RegisteredAt));
    }

    public PlaceOrderResponse PlaceOrder(int userId, PlaceOrderRequest request)
    {
        var user = _userRepository.GetById(userId) ?? throw new KeyNotFoundException("User not found.");
        var batchNum = new BatchNumber(request.BatchNumber);
        var charge = new Money(request.ChargeAmount, request.Currency);
        var order = user.PlaceOrder(batchNum, request.Quantity, charge);
        _userRepository.Save(user);
        return new PlaceOrderResponse(order.Id, order.PlacedAt);
    }

    public MakePaymentResponse MakePayment(int userId, MakePaymentRequest request)
    {
        var user = _userRepository.GetById(userId) ?? throw new KeyNotFoundException("User not found.");
        var amount = new Money(request.Amount, request.Currency);
        var payment = user.MakePayment(amount);
        _userRepository.Save(user);
        return new MakePaymentResponse(payment.Id, payment.Date);
    }

    public UserStatementResponse GetStatement(int userId)
    {
        var user = _userRepository.GetById(userId) ?? throw new KeyNotFoundException("User not found.");
        var orders = user.Orders.Select(o => new OrderDto(o.Id, o.PlacedAt, o.Batch.Value, o.Quantity, o.Charge.Amount));
        var payments = user.Payments.Select(p => new PaymentDto(p.Id, p.Date, p.Amount.Amount));
        return new UserStatementResponse(
            user.TotalCharged.Amount,
            user.TotalPaid.Amount,
            user.Balance.Amount,
            orders,
            payments);
    }
}