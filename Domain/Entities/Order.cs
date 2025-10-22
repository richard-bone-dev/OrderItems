using Api.Domain.Core;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public class Order : Entity<OrderId>
{
    public CustomerId UserId { get; private set; }
    public BatchId BatchId { get; private set; }
    public List<OrderDetail> OrderDetails { get; private set; } = new();

    private Order() { }

    private Order(OrderId id, CustomerId userId, BatchId batchId, List<OrderDetail> orderDetails)
    {
        Id = id;
        UserId = userId;
        BatchId = batchId;
        OrderDetails = orderDetails;
    }

    public static Order Create(CustomerId userId, BatchId batchId, List<OrderDetail> orderDetail)
    {
        return new Order(OrderId.New(), userId, batchId, orderDetail);
    }

    public void AddOrderDetail(OrderDetail detail)
    {
        if (detail == null)
            throw new ArgumentNullException(nameof(detail));
        OrderDetails.Add(detail);
    }

    public void RemoveOrderDetail(OrderDetail detail)
    {
        OrderDetails.Remove(detail);
    }
}