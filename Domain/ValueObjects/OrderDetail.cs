namespace Api.Domain.ValueObjects;

//public sealed class BatchNumber : IEquatable<BatchNumber>
//{
//    public int Value { get; }
//    public BatchNumber(int value)
//    {
//        if (value <= 0) throw new ArgumentException("Batch number must be positive.");
//        Value = value;
//    }
//    public bool Equals(BatchNumber other) => other?.Value == Value;
//    public override bool Equals(object obj) => Equals(obj as BatchNumber);
//    public override int GetHashCode() => Value.GetHashCode();
//}

public sealed class OrderDetail : IEquatable<OrderDetail>
{
    public Money Total { get; }
    public DateTime? OrderDate { get; }
    public DateTime? DueDate { get; }

    private OrderDetail() { }

    public OrderDetail(Money total, DateTime? orderDate = null, DateTime? dueDate = null)
    {
        Total = total ?? throw new ArgumentNullException(nameof(total));
        OrderDate = orderDate ?? DateTime.Now;
        DueDate = dueDate ?? DateTime.Now;
    }

    public bool Equals(OrderDetail? other)
        => other is not null && Total.Equals(other.Total) && DueDate == other.DueDate;

    public override bool Equals(object? obj) => Equals(obj as OrderDetail);
    public override int GetHashCode() => HashCode.Combine(Total, DueDate);
}
