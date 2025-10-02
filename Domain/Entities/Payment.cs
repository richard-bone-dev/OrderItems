using Api.Domain.Core;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public class Payment : Entity<PaymentId>
{
    public UserId UserId { get; private set; }
    public Money PaidAmount { get; private set; }
    public DateTime PaymentDate { get; private set; }

    private Payment() { }

    private Payment(PaymentId id, UserId userId, Money paidAmount, DateTime paymentDate)
    {
        Id = id;
        UserId = userId;
        PaidAmount = paidAmount;
        PaymentDate = paymentDate;
    }

    public static Payment Create(Guid userId, decimal paidAmount, DateTime? paymentDate = null)
    {
        if (paidAmount <= 0)
            throw new ArgumentException("Payment must be positive.");

        return new Payment(PaymentId.New(), new UserId(userId), new Money(paidAmount), paymentDate ?? DateTime.UtcNow);
    }
}