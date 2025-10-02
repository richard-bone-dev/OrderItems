namespace Api.Application.Orders.Dtos;

public record OrderDto(
    Guid OrderId,
    Guid UserId,
    Guid BatchId,
    int BatchNumber,
    Guid ProductTypeId,
    decimal UnitPrice,
    int Quantity,
    decimal Total,
    DateTime PlacedAt,
    DateTime? DueDate
);

/*
    {
      "orderId": "f61e00ac-9b18-4c94-a37a-55e98fc0e8da",
      "userId": "f61e00ac-9b18-4c94-a37a-55e98fc0e8da",
      "batchId": "f11c40f5-1c0a-49c0-9ec9-97ab8d3a8889",
      "batchNumber": 7,
      "productTypeId": "ad05f212-1ec7-4c53-bc24-4b76c2c7ef94",
      "placedAt": "2025-09-12T09:00:00Z",
      "dueDate": "2025-10-01T00:00:00Z",
      "total": 100.00
    }
*/