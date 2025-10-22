namespace Api.Application.Orders.Dtos;

using System.Collections.Generic;
using System.Linq;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

public static class OrderMapper
{
    public static OrderDto ToDto(Order order, BatchNumber batchNumber) => new(
        order.Id.Value,
        order.CustomerId.Value,
        order.BatchId.Value,
        batchNumber.Value,
        order.OrderDetails.First().ProductTypeId.Value,
        order.OrderDetails.First().UnitPrice.Amount,
        order.OrderDetails.First().Quantity,
        order.OrderDetails.First().Total.Amount,
        order.OrderDetails.First().PlacedAt,
        order.OrderDetails.First().DueDate);

    public static OrderDto ToDto(this Order order, Dictionary<BatchId, BatchNumber> batchMap)
    {
        var batchNumber = batchMap.TryGetValue(order.BatchId, out var bn)
            ? bn.Value
            : 0;

        return new OrderDto(
            order.Id.Value,
            order.CustomerId.Value,
            order.BatchId.Value,
            batchNumber,
            order.OrderDetails.First().ProductTypeId.Value,
            order.OrderDetails.First().UnitPrice.Amount,
            order.OrderDetails.First().Quantity,
            order.OrderDetails.First().Total.Amount,
            order.OrderDetails.First().PlacedAt,
            order.OrderDetails.First().DueDate);
    }
}
