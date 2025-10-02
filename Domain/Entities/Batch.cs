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

    private readonly List<BatchStock> _stocks = new();
    public IReadOnlyCollection<BatchStock> Stocks => _stocks.AsReadOnly();

    public void ReserveStock(ProductTypeId productTypeId, int quantity)
    {
        var line = _stocks.SingleOrDefault(s => s.ProductTypeId == productTypeId)
            ?? throw new InvalidOperationException("Product not found in batch stock.");

        line.Reserve(quantity);
    }

    private Batch() { }

    private Batch(BatchId id, BatchNumber number, DateTime createdAt, bool isActive)
    {
        Id = id;
        Number = number;
        CreatedAt = createdAt;
        IsActive = isActive;
    }

    public static Batch Create(BatchNumber number)
        => new(BatchId.New(), number, DateTime.UtcNow, true);

    public Order AddOrder(UserId userId, ProductTypeId productTypeId, Money total, DateTime placedAt, DateTime? dueDate = null, int quantity = 1)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot add orders to a closed batch.");

        ReserveStock(productTypeId, quantity);

        var detail = new OrderDetail(productTypeId, total, quantity, placedAt, dueDate);
        var order = Order.Create(userId, Id, detail);
        _orders.Add(order);

        return order;
    }

    public void Close() => IsActive = false;
}

public class BatchStock
{
    public ProductTypeId ProductTypeId { get; private set; }
    public int Available { get; private set; }

    private BatchStock() { }

    public BatchStock(ProductTypeId productTypeId, int quantity)
    {
        ProductTypeId = productTypeId;
        Available = quantity;
    }

    public void Reserve(int quantity)
    {
        if (quantity > Available)
            throw new InvalidOperationException("Insufficient batch stock for this product type.");

        Available -= quantity;
    }
}