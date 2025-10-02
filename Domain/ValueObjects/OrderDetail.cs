using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public sealed class OrderDetail : IEquatable<OrderDetail>
{
    public ProductTypeId ProductTypeId { get; }
    public Money UnitPrice { get; }
    public int Quantity { get; }
    public Money Total => new(UnitPrice.Amount * Quantity);
    public DateTime PlacedAt { get; }
    public DateTime? DueDate { get; }

    private OrderDetail() { }

    public OrderDetail(ProductTypeId productTypeId, Money unitPrice, int quantity, DateTime placedAt, DateTime? dueDate = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.");

        ProductTypeId = productTypeId ?? throw new ArgumentNullException(nameof(productTypeId));
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Quantity = quantity;
        PlacedAt = placedAt;
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