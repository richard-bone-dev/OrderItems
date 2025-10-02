using Api.Application.Abstractions;
using Api.Application.Users.Dtos;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Users.Queries;

public record GetUsersQuery();
public record GetUserStatementQuery(UserId UserId);

public class GetUsersHandler : IQueryHandler<GetUsersQuery, IEnumerable<UserDto>>
{
    private readonly ApplicationDbContext _db;

    public GetUsersHandler(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery query, CancellationToken ct = default)
    {
        return await _db.Users
            .Select(u => new UserDto(u.Id.Value, u.Name.Value, u.RegisteredAt))
            .ToListAsync(ct);
    }
}