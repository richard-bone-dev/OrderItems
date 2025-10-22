using Api.Domain.Core;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public class Batch : Entity<BatchId>
{
    public BatchNumber Number { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();


    private BatchStock _stock;
    public BatchStock Stock => _stock;

    private Batch() { }

    private Batch(BatchId id, BatchNumber number, BatchStock stock, DateTime createdAt, bool isActive)
    {
        Id = id;
        Number = number;
        CreatedAt = createdAt;
        IsActive = isActive;
        _stock = stock;
    }

    public static Batch Create(BatchNumber number, int initialStock = 0)
        => new(BatchId.New(), number, new BatchStock(initialStock), DateTime.UtcNow, true);

    public Order AddOrder(CustomerId userId, ProductTypeId productTypeId, Money total, DateTime placedAt, DateTime? dueDate = null, int quantity = 1)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot add orders to a closed batch.");

        ReserveStock(quantity);

        var detail = new OrderDetail(productTypeId, total, placedAt, quantity, dueDate);
        var order = Order.Create(userId, Id, [detail]);
        _orders.Add(order);

        return order;
    }

    public void ReserveStock(int quantity) => _stock.Reserve(quantity);

    public void Close() => IsActive = false;
}


public class BatchStock : ValueObject
{
    public int Available { get; private set; }

    private BatchStock() { }

    public BatchStock(int available)
    {
        if (available < 0) throw new ArgumentOutOfRangeException(nameof(available));
        Available = available;
    }

    public void Reserve(int quantity)
    {
        if (quantity > Available)
            throw new InvalidOperationException("Insufficient stock.");

        Available -= quantity;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Available;
    }
}