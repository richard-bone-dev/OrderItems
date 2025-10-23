using Api.Application.Abstractions;
using Api.Application.Payments.Commands;
using Api.Application.Payments.Dtos;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

public class RecordPaymentHandler : ICommandHandlerAsync<RecordPaymentCommand, PaymentDto>
{
    private readonly ICustomerRepository _repo;
    private readonly IUnitOfWork _unitOfWork;

    public RecordPaymentHandler(ICustomerRepository repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentDto> HandleAsync(RecordPaymentCommand cmd, CancellationToken ct = default)
    {
        var customer = await _repo.GetByIdAsync(new CustomerId(cmd.CustomerId), ct)
                   ?? throw new KeyNotFoundException("User not found.");

        var payment = Payment.Create(cmd.CustomerId, cmd.Amount, cmd.PaymentDate);
        customer.AddPayment(payment);

        await _unitOfWork.SaveChangesAsync(ct);

        return new PaymentDto(payment.Id.Value, customer.Id.Value, cmd.Amount, payment.PaymentDate);
    }
}