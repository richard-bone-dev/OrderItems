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
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerCommand cmd, CancellationToken ct)
    {
        var result = await _createCustomer.Handle(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { userId = result.Id }, result);
    }

    /// <summary>
    /// List all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers(CancellationToken ct)
    {
        var users = await _getCustomers.Handle(new GetCustomersQuery(), ct);
        return Ok(users);
    }

    /// <summary>
    /// Get user by Id
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid userId, CancellationToken ct)
    {
        var users = await _getCustomers.Handle(new GetCustomersQuery(), ct);
        var user = users.FirstOrDefault(u => u.Id == userId);

        if (user is null) return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Get user financial statement
    /// </summary>
    [HttpGet("{userId:guid}/statement")]
    public async Task<ActionResult<CustomerStatementResponse>> GetStatement(Guid userId, CancellationToken ct)
    {
        var response = await _getStatement.Handle(new GetCustomerStatementQuery(new CustomerId(userId)), ct);
        return Ok(response);
    }
}
