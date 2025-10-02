using Api.Application.Abstractions;
using Api.Application.Users.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Users.Commands.Handlers;

public class CreateUserHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _repository;

    public CreateUserHandler(IUserRepository repository) => _repository = repository;

    public async Task<UserDto> Handle(CreateUserCommand command, CancellationToken ct = default)
    {
        var user = User.Register(new UserName(command.Name));

        await _repository.AddAsync(user, ct);
        await _repository.SaveChangesAsync(ct);

        return new UserDto(
            user.Id.Value,
            user.Name.Value,
            user.RegisteredAt
        );
    }
}