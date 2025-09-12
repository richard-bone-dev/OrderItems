using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Interfaces;

public interface IOrderRepository
{
    Order GetById(OrderId orderId);
    IEnumerable<Order> GetByUserId(UserId userId);
    void Save(Order order);
}
