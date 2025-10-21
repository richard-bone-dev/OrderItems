using Api.Application.Abstractions;
using Api.Application.Payments.Commands;
using Api.Application.Payments.Dtos;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

public class RecordPaymentHandler : ICommandHandler<RecordPaymentCommand, PaymentDto>
{
    private readonly IUserRepository _repo;

    public RecordPaymentHandler(IUserRepository repo) => _repo = repo;

    public async Task<PaymentDto> Handle(RecordPaymentCommand cmd, CancellationToken ct = default)
    {
        var user = await _repo.GetByIdAsync(new CustomerId(cmd.UserId), ct)
                   ?? throw new KeyNotFoundException("User not found.");

        var payment = Payment.Create(cmd.UserId, cmd.Amount, cmd.PaymentDate);
        user.AddPayment(payment);

        await _repo.SaveChangesAsync(ct);

        return new PaymentDto(payment.Id.Value, user.Id.Value, cmd.Amount, payment.PaymentDate);
    }
}