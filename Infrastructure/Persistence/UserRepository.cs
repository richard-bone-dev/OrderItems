using Api.Application.Interfaces;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    public UserRepository(ApplicationDbContext db) => _db = db;

    public User GetById(UserId userId)
        => _db.Users
              .Include(u => u.Orders)
              .Include(u => u.Payments)
              .Single(u => u.Id == userId);

    public User GetByName(string name)
        => _db.Users
              .Include(u => u.Orders)
              .Include(u => u.Payments)
              .Single(u => u.Name == name);

    public IEnumerable<User> GetAll()
        => _db.Users.AsNoTracking().ToList();

    public void Save(User user)
    {
        var exists = _db.Users.Any(u => u.Id == user.Id || u.Name.ToLower() == user.Name.ToLower());
        if (!exists)
        {
            _db.Users.Add(user);
            _db.SaveChanges();
        }
        //else
        //    throw new InvalidOperationException($"User '{user.Name}' already exists.");
    }
}
