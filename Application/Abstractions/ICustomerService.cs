using Api.Application.Customers.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Interfaces;

public interface ICustomerService
{
    CustomerDto CreateCustomer(CreateCustomerRequest request);
    IEnumerable<CustomerDto> ListCustomers();
    CustomerStatementResponse GetStatement(CustomerId customerId);
}