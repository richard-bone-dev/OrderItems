using Api.Application.Dtos;
using Api.Application.Interfaces;
using Api.Domain.ValueObjects;

namespace Api.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUserRepository _userRepository;
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(
        IUserRepository userRepository,
        IPaymentRepository paymentRepository)
    {
        _userRepository = userRepository;
        _paymentRepository = paymentRepository;
    }

    public MakePaymentResponse MakePayment(UserId userId, MakePaymentRequest request)
    {
        var user = _userRepository.GetById(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var money = new Money(request.Amount);

        var payment = user.MakePayment(userId, money, new Money(0), request.PaymentDate);

        _userRepository.Save(user);

        return new MakePaymentResponse(
            payment.Id,
            payment.PaidAmount.Amount,
            payment.PaymentDate
        );
    }

    public IEnumerable<PaymentDto> GetUserPayments(UserId userId)
    {
        return _paymentRepository.GetByUserId(userId)
            .Select(p => new PaymentDto(p.Id, p.PaidAmount.Amount, p.PaymentDate))
            .OrderByDescending(p => p.PaymentDate);
    }
}