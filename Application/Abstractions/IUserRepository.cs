using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}