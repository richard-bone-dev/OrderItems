using Api.Application.Orders.Dtos;
using Api.Application.Payments.Dtos;

namespace Api.Application.Users.Dtos;

public record UserStatementResponse(
    Guid UserId,
    string UserName,
    decimal TotalCharged,
    decimal TotalPaid,
    decimal Balance,
    IEnumerable<OrderDto> Orders,
    IEnumerable<PaymentDto> Payments
);

/*
    {
      "userId": "59cee826-fb69-46a7-9518-009213952978",
      "userName": "Alice",
      "totalCharged": 200.00,
      "totalPaid": 150.00,
      "balance": 50.00,
      "orders": [
        {
          "id": "f61e00ac-9b18-4c94-a37a-55e98fc0e8da",
          "batchId": "f11c40f5-1c0a-49c0-9ec9-97ab8d3a8889",
          "batchNumber": 7,
          "productTypeId": "ad05f212-1ec7-4c53-bc24-4b76c2c7ef94",
          "placedAt": "2025-09-12T09:00:00Z",
          "dueDate": "2025-10-01T00:00:00Z",
          "total": 100.00
        }
      ],
      "payments": [
        {
          "id": "3a5dc27c-02d4-4a1d-bc55-2b5c9f3f8190",
          "userId": "59cee826-fb69-46a7-9518-009213952978",
          "amount": 50.00,
          "paymentDate": "2025-09-12T08:45:00Z"
        }
      ]
    }
*/