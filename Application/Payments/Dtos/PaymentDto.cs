namespace Api.Application.Payments.Dtos;

public record PaymentHistoryDto(
    string State,
    DateTime ChangedAt,
    string? Reason,
    string? ExternalReference
);

public record PaymentDto(
    Guid PaymentId,
    Guid UserId,
    decimal? PaidAmount,
    DateTime PaymentDate
);

/*
    {
      "id": "3a5dc27c-02d4-4a1d-bc55-2b5c9f3f8190",
      "userId": "59cee826-fb69-46a7-9518-009213952978",
      "amount": 50.00,
      "paymentDate": "2025-09-12T08:45:00Z"
    }
*/

public record PaymentDetailDto(
    Guid Id,
    Guid UserId,
    decimal? Amount,
    DateTime PaymentDate
);

/*
    {
      "id": "75b8fa54-ef14-41d3-b1cf-68a3f0a87a92",
      "userId": "59cee826-fb69-46a7-9518-009213952978",
      "amount": 75.00,
      "paymentDate": "2025-09-15T14:00:00Z"
    }
*/

public record PaymentSnapshotDto(
    Guid Id,
    Guid UserId,
    decimal Amount,
    DateTime PaymentDate
);

/*
    {
      "id": "75b8fa54-ef14-41d3-b1cf-68a3f0a87a92",
      "userId": "59cee826-fb69-46a7-9518-009213952978",
      "amount": 75.00,
      "paymentDate": "2025-09-15T14:00:00Z"
    }
*/