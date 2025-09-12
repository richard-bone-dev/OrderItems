using Api.Domain.Core;
using Api.Domain.Events;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;


// --- Aggregate Root: User ---
public class User : Entity<UserId>
{
    public string Name { get; private set; }
    public bool Preferred { get; private set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    private User(string name)
    {
        Name = name;
        Preferred = false;
    }

    private User(UserId id, string name, bool preferred)
    {
        Id = id;
        Name = name;
        Preferred = preferred;
    }

    public static User Create(UserId id, string name, bool preferred)
    {
        return new User(id, name, preferred);
    }

    public static User Register(string name)
    {
        var user = new User(name)
        {
            Id = new(Guid.NewGuid())
        };
        return user;
    }

    public void ApplyExternalPayment(Payment payment)
    {
        if (payment.UserId != Id) throw new InvalidOperationException("Payment user mismatch.");

        Payments.Add(payment);
        DomainEvents.Raise(new PaymentRecorded(this, payment));
    }

    public Order PlaceOrder(UserId userId, BatchNumber batch, ProductTypeId productTypeId, OrderDetail orderDetail)
    {
        var order = Order.Create(userId, batch, productTypeId, orderDetail);

        Orders.Add(order);
        DomainEvents.Raise(new OrderPlaced(this, order));
        return order;
    }

    public Payment MakePayment(UserId userId, Money paid, Money remaining, DateTime? paymentDate)
    {
        var payment = Payment.Create(userId, paid, remaining, paymentDate);

        Payments.Add(payment);
        DomainEvents.Raise(new PaymentRecorded(this, payment));
        return payment;
    }

    public Money TotalCharged => Orders.Aggregate(new Money(0m), (sum, o) => sum.Add(o.OrderDetail.Total));
    public Money TotalPaid => Payments.Aggregate(new Money(0m), (sum, p) => sum.Add(p.PaidAmount));
    public Money Balance => TotalCharged.Subtract(TotalPaid);
}
