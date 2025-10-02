using Api.Application.Users.Commands;
using Api.Application.Users.Commands.Handlers;
using Api.Application.Users.Dtos;
using Api.Application.Users.Queries;
using Api.Application.Users.Queries.Handlers;
using Api.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly CreateUserHandler _createUser;
    private readonly GetUsersHandler _getUsers;
    private readonly GetUserStatementHandler _getStatement;

    public UsersController(
        CreateUserHandler createUser,
        GetUsersHandler getUsers,
        GetUserStatementHandler getStatement)
    {
        _createUser = createUser;
        _getUsers = getUsers;
        _getStatement = getStatement;
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserCommand cmd, CancellationToken ct)
    {
        var result = await _createUser.Handle(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { userId = result.Id }, result);
    }

    /// <summary>
    /// List all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(CancellationToken ct)
    {
        var users = await _getUsers.Handle(new GetUsersQuery(), ct);
        return Ok(users);
    }

    /// <summary>
    /// Get user by Id
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid userId, CancellationToken ct)
    {
        var users = await _getUsers.Handle(new GetUsersQuery(), ct);
        var user = users.FirstOrDefault(u => u.Id == userId);

        if (user is null) return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Get user financial statement
    /// </summary>
    [HttpGet("{userId:guid}/statement")]
    public async Task<ActionResult<UserStatementResponse>> GetStatement(Guid userId, CancellationToken ct)
    {
        var response = await _getStatement.Handle(new GetUserStatementQuery(new UserId(userId)), ct);
        return Ok(response);
    }
}
