using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public sealed class OrderDetail : IEquatable<OrderDetail>
{
    public ProductTypeId ProductTypeId { get; }
    public Money UnitPrice { get; }
    public int Quantity { get; }
    public DateTime PlacedAt { get; }
    public DateTime? DueDate { get; }
    public Money Total => new(UnitPrice.Amount * Quantity);

    private OrderDetail() { }

    public OrderDetail(ProductTypeId productTypeId, Money unitPrice, DateTime placedAt, int quantity = 1, DateTime? dueDate = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.");

        ProductTypeId = productTypeId ?? throw new ArgumentNullException(nameof(productTypeId));
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        PlacedAt = placedAt;
        Quantity = quantity;
        DueDate = dueDate;
    }

    public bool Equals(OrderDetail? other)
    {
        return other is not null
            && ProductTypeId == other.ProductTypeId
            && UnitPrice.Equals(other.UnitPrice)
            && Quantity == other.Quantity
            && PlacedAt == other.PlacedAt
            && DueDate == other.DueDate;
    }

    public override bool Equals(object? obj) => Equals(obj as OrderDetail);
    public override int GetHashCode() => HashCode.Combine(ProductTypeId, UnitPrice, Quantity, PlacedAt, DueDate);
}