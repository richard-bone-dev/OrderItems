using Api.Domain.Core;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public class Order : Entity<OrderId>
{
    public UserId UserId { get; private set; }
    public BatchId BatchId { get; private set; }
    public OrderDetail OrderDetail { get; private set; }

    private Order() { }

    private Order(OrderId id, UserId userId, BatchId batchId, OrderDetail orderDetail)
    {
        Id = id;
        UserId = userId;
        BatchId = batchId;
        OrderDetail = orderDetail;
    }

    public static Order Create(UserId userId, BatchId batchId, OrderDetail orderDetail)
    {
        return new Order(OrderId.New(), userId, batchId, orderDetail);
    }
}