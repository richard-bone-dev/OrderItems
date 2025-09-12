namespace Api.Domain.ValueObjects;

public enum PaymentState
{
    Pending,
    Settled,
    Failed,
    Refunded
}
