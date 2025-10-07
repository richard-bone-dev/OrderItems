using Api.Application.Abstractions;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    public UserRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _db.Users.AddAsync(user, ct);

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
        => await _db.Users.Include(u => u.Orders).Include(u => u.Payments).FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken ct = default)
        => await _db.Users.Include(u => u.Orders).Include(u => u.Payments).ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}