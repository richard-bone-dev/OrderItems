namespace Api.Application.Batches.Dtos;

public record BatchDto(
    Guid Id,
    int Number,
    DateTime CreatedAt,
    bool IsActive
);

/*
    {
      "id": "b72b735f-623d-41fb-9f1a-13f781f10f6c",
      "number": 8,
      "createdAt": "2025-09-12T08:00:00Z",
      "isActive": true
    }
*/