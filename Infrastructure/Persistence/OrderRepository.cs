using Api.Application.Interfaces;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;
    public OrderRepository(ApplicationDbContext db) => _db = db;

    public Order GetById(OrderId orderId)
        => _db.Orders.Single(p => p.Id == orderId);

    public IEnumerable<Order> GetByUserId(UserId userId)
        => _db.Orders
              .Where(p => p.UserId == userId)
              .AsNoTracking()
              .ToList();

    public void Save(Order order)
    {
        var existing = _db.Orders.Find(order.Id);
        if (existing == null)
            throw new InvalidOperationException("Order not found.");

        _db.Entry(existing).CurrentValues.SetValues(order);
        _db.SaveChanges();

        //if (_db.Entry(order).State == EntityState.Detached)
        //    _db.Orders.Add(order);

        //_db.SaveChanges();
    }
}
