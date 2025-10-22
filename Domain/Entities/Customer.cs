using Api.Domain.Core;
using Api.Domain.Entities;
using Api.Domain.Events;
using Api.Domain.ValueObjects;

public class Customer : Entity<CustomerId>
{
    public CustomerName Name { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private readonly List<Payment> _payments = new();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public Money TotalCharged =>
        new Money(_orders
            .SelectMany(o => o.OrderDetails)
            .Sum(d => d.Total.Amount));

    public Money TotalPaid => _payments.Aggregate(new Money(0m), (sum, p) => sum.Add(p.PaidAmount));
    public Money Balance => TotalCharged.Subtract(TotalPaid);

    private Customer() { }

    private Customer(CustomerId id, CustomerName name, DateTime registeredAt)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        RegisteredAt = registeredAt;
    }

    public static Customer Register(CustomerName name)
        => new(CustomerId.New(), name, DateTime.UtcNow);

    public void AddPayment(Payment payment)
    {
        if (payment.CustomerId.Value != Id.Value)
            throw new InvalidOperationException("Payment customer mismatch.");

        _payments.Add(payment);
        DomainEvents.Raise(new PaymentRecorded(this, payment));
    }

    public void AddOrder(Order order)
    {
        if (order.CustomerId.Value != Id.Value)
            throw new InvalidOperationException("Order customer mismatch.");

        _orders.Add(order);
    }
}