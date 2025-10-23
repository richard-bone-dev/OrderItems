using Api.Application.Abstractions;
using Api.Application.Customers.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Customers.Commands.Handlers;

public class CreateCustomerHandler : ICommandHandlerAsync<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> HandleAsync(CreateCustomerCommand command, CancellationToken ct = default)
    {
        var user = Customer.Register(new CustomerName(command.Name));

        await _repository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CustomerDto(
            user.Id.Value,
            user.Name.Value,
            user.RegisteredAt
        );
    }
}