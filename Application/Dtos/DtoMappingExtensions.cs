using Api.Application.Orders.Dtos;
using Api.Application.Payments.Dtos;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Dtos;

public static class DtoMappingExtensions
{
    public static PaymentDto ToDto(this Payment p) => new(
        p.Id.Value,
        p.UserId.Value,
        p.PaidAmount.Amount,
        p.PaymentDate
    );

    public static OrderDto ToDto(this Order o, int batchNumber) => new(
        o.Id.Value,
        o.UserId.Value,
        o.BatchId.Value,
        batchNumber,
        o.OrderDetail.ProductTypeId.Value,
        o.OrderDetail.UnitPrice.Amount,
        o.OrderDetail.Quantity,
        o.OrderDetail.Total.Amount,
        o.OrderDetail.PlacedAt,
        o.OrderDetail.DueDate
    );

    public static OrderDto ToDto(this Order order, Dictionary<BatchId, BatchNumber> batchMap)
    {
        var batchNumber = batchMap.TryGetValue(order.BatchId, out var bn)
            ? bn.Value
            : 0;

        return new OrderDto(
            order.Id.Value,
            order.UserId.Value,
            order.BatchId.Value,
            batchNumber,
            order.OrderDetail.ProductTypeId.Value,
            order.OrderDetail.UnitPrice.Amount,
            order.OrderDetail.Quantity,
            order.OrderDetail.Total.Amount,
            order.OrderDetail.PlacedAt,
            order.OrderDetail.DueDate
        );
    }
}