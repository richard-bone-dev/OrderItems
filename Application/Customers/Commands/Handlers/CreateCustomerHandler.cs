using Api.Application.Abstractions;
using Api.Application.Customers.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Customers.Commands.Handlers;

public class CreateCustomerHandler : ICommandHandlerAsync<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _repository;

    public CreateCustomerHandler(ICustomerRepository repository) => _repository = repository;

    public async Task<CustomerDto> HandleAsync(CreateCustomerCommand command, CancellationToken ct = default)
    {
        var user = Customer.Register(new CustomerName(command.Name));

        await _repository.AddAsync(user, ct);
        await _repository.SaveChangesAsync(ct);

        return new CustomerDto(
            user.Id.Value,
            user.Name.Value,
            user.RegisteredAt
        );
    }
}