using Api.Application.Abstractions;
using Api.Application.Customers.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Customers.Commands.Handlers;

public class CreateCustomerHandler : ICommandHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IUserRepository _repository;

    public CreateCustomerHandler(IUserRepository repository) => _repository = repository;

    public async Task<CustomerDto> Handle(CreateCustomerCommand command, CancellationToken ct = default)
    {
        var user = Customer.Register(new UserName(command.Name));

        await _repository.AddAsync(user, ct);
        await _repository.SaveChangesAsync(ct);

        return new CustomerDto(
            user.Id.Value,
            user.Name.Value,
            user.RegisteredAt
        );
    }
}