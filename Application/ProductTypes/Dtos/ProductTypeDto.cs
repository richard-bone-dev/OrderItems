namespace Api.Application.ProductTypes.Dtos;

public record ProductTypeDto(
    Guid Id,
    string Name,
    decimal? UnitPrice
);

/*
    {
      "id": "c1a38f51-64ab-4c41-9a6b-4c73fb6f7c3e",
      "name": "Premium Widget",
      "unitPrice": 19.99
    }
*/