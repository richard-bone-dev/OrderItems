using Api.Application.Customers.Commands;
using Api.Application.Customers.Commands.Handlers;
using Api.Application.Customers.Dtos;
using Api.Application.Customers.Queries;
using Api.Application.Customers.Queries.Handlers;
using Api.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly CreateCustomerHandler _createCustomer;
    private readonly GetCustomersHandler _getCustomers;
    private readonly GetCustomerStatementHandler _getStatement;

    public CustomersController(
        CreateCustomerHandler createCustomer,
        GetCustomersHandler getCustomers,
        GetCustomerStatementHandler getStatement)
    {
        _createCustomer = createCustomer;
        _getCustomers = getCustomers;
        _getStatement = getStatement;
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateAsync([FromBody] CreateCustomerCommand cmd, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _createCustomer.HandleAsync(cmd, ct);
        return CreatedAtAction(nameof(GetByIdAsync), new { customerId = result.Id }, result);
    }

    /// <summary>
    /// List all customers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomersAsync(CancellationToken ct)
    {
        var customers = await _getCustomers.HandleAsync(new GetCustomersQuery(), ct);
        return Ok(customers);
    }

    /// <summary>
    /// Get customer by Id
    /// </summary>
    [HttpGet("{customerId:guid}")]
    public async Task<ActionResult<CustomerDto>> GetByIdAsync(Guid customerId, CancellationToken ct)
    {
        var customers = await _getCustomers.HandleAsync(new GetCustomersQuery(), ct);
        var customer = customers.FirstOrDefault(u => u.Id == customerId);

        if (customer is null) return NotFound();
        return Ok(customer);
    }

    /// <summary>
    /// Get customer financial statement
    /// </summary>
    [HttpGet("{customerId:guid}/statement")]
    public async Task<ActionResult<CustomerStatementResponse>> GetStatementAsync(Guid customerId, CancellationToken ct)
    {
        try
        {
            var response = await _getStatement.HandleAsync(new GetCustomerStatementQuery(new CustomerId(customerId)), ct);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
