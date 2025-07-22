namespace Api.Domain;

public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public Money(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Money amount cannot be negative.");
        Amount = amount;
    }

    public Money Add(Money other)
    {
        return new Money(Amount + other.Amount);
    }

    public Money Subtract(Money other)
    {
        return new Money(Amount - other.Amount);
    }

    public bool Equals(Money other) => other is not null && Amount == other.Amount;
    public override bool Equals(object obj) => Equals(obj as Money);
    public override int GetHashCode() => HashCode.Combine(Amount);
}

public sealed class BatchNumber : IEquatable<BatchNumber>
{
    public int Value { get; }
    public BatchNumber(int value)
    {
        if (value <= 0) throw new ArgumentException("Batch number must be positive.");
        Value = value;
    }
    public bool Equals(BatchNumber other) => other?.Value == Value;
    public override bool Equals(object obj) => Equals(obj as BatchNumber);
    public override int GetHashCode() => Value.GetHashCode();
}

// --- Entities & Aggregates ---
public class Order : Entity<int>
{
    public DateTime PlacedAt { get; private set; }
    public BatchNumber Batch { get; private set; }
    public decimal? Quantity { get; private set; }
    public Money Charge { get; private set; }     // total charge for this order

    private Order() { }

    internal Order(DateTime placedAt, BatchNumber batch, decimal? quantity, Money charge)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        if (charge.Amount < 0) throw new ArgumentException("Charge cannot be negative.");
        //Id = id;
        PlacedAt = placedAt;
        Batch = batch;
        Quantity = quantity;
        Charge = charge;
    }

    public static Order Create(BatchNumber batch, decimal? quantity, Money charge)
    {
        //var id = Guid.NewGuid();
        return new Order(DateTime.UtcNow, batch, quantity, charge);
    }
}

public class Payment : Entity<int>
{
    public Money Amount { get; private set; }
    public DateTime Date { get; private set; }

    private Payment() { }

    internal Payment(Money amount, DateTime date)
    {
        //Id = id;
        Amount = amount;
        Date = date;
    }

    public static Payment Create(Money amount, DateTime? date = null)
    {
        if (amount.Amount <= 0) throw new ArgumentException("Payment amount must be positive.");
        var id = Guid.NewGuid();
        return new Payment(amount, date ?? DateTime.UtcNow);
    }
}

// --- Aggregate Root: User ---
public class User : Entity<int>
{
    public string Name { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    private readonly List<Order> _orders = new();
    private readonly List<Payment> _payments = new();

    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private User(string name)
    {
        //Id = id;
        Name = name;
        RegisteredAt = DateTime.UtcNow;
    }

    public static User Register(string name) => new User(name);

    public Order PlaceOrder(BatchNumber batch, decimal? quantity, Money charge)
    {
        var order = Order.Create(batch, quantity, charge);
        _orders.Add(order);
        DomainEvents.Raise(new OrderPlaced(this, order));
        return order;
    }

    public Payment MakePayment(Money amount)
    {
        var payment = Payment.Create(amount);
        _payments.Add(payment);
        DomainEvents.Raise(new PaymentRecorded(this, payment));
        return payment;
    }

    public Money TotalCharged => Orders.Aggregate(new Money(0m), (sum, o) => sum.Add(o.Charge));
    public Money TotalPaid => Payments.Aggregate(new Money(0m), (sum, p) => sum.Add(p.Amount));
    public Money Balance => TotalCharged.Subtract(TotalPaid);
}

// --- Domain Events ---
public abstract class DomainEvent { }

public static class DomainEvents
{
    public static event Action<DomainEvent> Handlers;
    public static void Register(Action<DomainEvent> handler) => Handlers += handler;
    public static void Raise(DomainEvent domainEvent) => Handlers?.Invoke(domainEvent);
}

public sealed class OrderPlaced : DomainEvent
{
    public User User { get; }
    public Order Order { get; }
    public OrderPlaced(User user, Order order) => (User, Order) = (user, order);
}

public sealed class PaymentRecorded : DomainEvent
{
    public User User { get; }
    public Payment Payment { get; }
    public PaymentRecorded(User user, Payment payment) => (User, Payment) = (user, payment);
}

// Simple ID generator for PoC
public static class IdGenerator
{
    private static int _order = 0;
    private static int _payment = 0;
    public static int NextOrderId() => ++_order;
    public static int NextPaymentId() => ++_payment;
}

// --- Domain Service for Batch Assignment ---
public interface IBatchAssignmentService
{
    BatchNumber GetCurrentBatch();
    void AdvanceToNextBatch();
}

// --- Infrastructure: Base Entity ---
public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
}
