using Api.Domain.Core;
using Api.Domain.Entities;
using Api.Domain.Events;
using Api.Domain.ValueObjects;

public class User : Entity<UserId>
{
    public UserName Name { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private readonly List<Payment> _payments = new();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public Money TotalCharged => _orders.Aggregate(new Money(0m), (sum, o) => sum.Add(o.OrderDetail.Total));
    public Money TotalPaid => _payments.Aggregate(new Money(0m), (sum, p) => sum.Add(p.PaidAmount));
    public Money Balance => TotalCharged.Subtract(TotalPaid);

    private User() { }

    private User(UserId id, UserName name, DateTime registeredAt)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        RegisteredAt = registeredAt;
    }

    public static User Register(UserName name)
        => new(UserId.New(), name, DateTime.UtcNow);

    public void AddPayment(Payment payment)
    {
        if (payment.UserId.Value != Id.Value)
            throw new InvalidOperationException("Payment user mismatch.");

        _payments.Add(payment);
        DomainEvents.Raise(new PaymentRecorded(this, payment));
    }

    public void AddOrder(Order order)
    {
        if (order.UserId.Value != Id.Value)
            throw new InvalidOperationException("Order user mismatch.");

        _orders.Add(order);
    }
}