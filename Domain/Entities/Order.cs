using Api.Domain.Core;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public class Order : Entity<OrderId>
{
    public CustomerId CustomerId { get; private set; }
    public BatchId BatchId { get; private set; }

    public List<OrderDetail> _orderDetails;
    public IReadOnlyCollection<OrderDetail> OrderDetails => _orderDetails;

    private Order() { }

    private Order(OrderId id, CustomerId customerId, BatchId batchId, List<OrderDetail> orderDetails)
    {
        Id = id;
        CustomerId = customerId;
        BatchId = batchId;
        _orderDetails = orderDetails;
    }

    public static Order Create(CustomerId customerId, BatchId batchId, List<OrderDetail> orderDetail)
    {
        return new Order(OrderId.New(), customerId, batchId, orderDetail);
    }

    public void AddOrderDetail(OrderDetail detail)
    {
        if (detail == null)
            throw new ArgumentNullException(nameof(detail));
        _orderDetails.Add(detail);
    }

    public void RemoveOrderDetail(OrderDetail detail)
    {
        _orderDetails.Remove(detail);
    }
}