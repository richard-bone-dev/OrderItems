namespace Api.Application.Customers.Dtos;

public record CustomerDto(Guid Id, string Name, DateTime RegisteredAt);

/*
    {
      "id": "59cee826-fb69-46a7-9518-009213952978",
      "name": "Alice",
      "registeredAt": "2025-09-12T08:00:00Z"
    }
*/


public class CreateCustomerRequest { public string Name { get; set; } = string.Empty; }