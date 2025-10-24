using Api.Application.Customers.Dtos;
using Api.Application.Orders.Dtos;
using Api.Application.Payments.Dtos;
using Api.Application.Interfaces;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public UserService(
        IUserRepository userRepository,
        IPaymentRepository paymentRepository,
        IProductTypeRepository productTypeRepository)
    {
        _userRepository = userRepository;
    }

    public IEnumerable<UserDto> ListUsers()
    {
        var result = _userRepository.GetAll()
            .Select(ToUserDto);
        return result;
    }

    public UserDto CreateUser(CreateUserRequest request)
    {
        var user = User.Register(request.Name);
        _userRepository.Save(user);
        return ToUserDto(user);
    }

    public UserStatementResponse GetStatement(UserId userId)
    {
        var user = _userRepository.GetById(userId) ?? throw new KeyNotFoundException("User not found.");

        var orders = user.Orders
            .Select(ToOrderDto)
            .OrderByDescending(o => o.OrderDate);

        var payments = user.Payments
            .Select(p => new PaymentDto(p.Id, p.PaidAmount.Amount, p.PaymentDate))
            .OrderByDescending(p => p.PaymentDate);

        return new UserStatementResponse(
            user.Name,
            user.TotalCharged.Amount,
            user.TotalPaid.Amount,
            user.Balance.Amount,
            orders,
            payments);
    }

    private static UserDto ToUserDto(User user)
    {
        return new UserDto(user.Id, user.Name, user.Preferred, user.Orders.Sum(c => c.OrderDetail.Total.Amount));
    }

    private static OrderDto ToOrderDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.BatchNumber,
            order.ProductTypeId,
            order.OrderDetail.OrderDate,
            order.OrderDetail.Total.Amount
        );
    }
}