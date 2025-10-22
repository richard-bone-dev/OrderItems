using Api.Application.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Interfaces;

public interface IUserService
{
    UserDto CreateUser(CreateUserRequest request);
    IEnumerable<UserDto> ListUsers();
    UserStatementResponse GetStatement(UserId userId);
}