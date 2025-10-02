namespace Api.Application.Users.Dtos;

public record UserDto(Guid Id, string Name, DateTime RegisteredAt);

/*
    {
      "id": "59cee826-fb69-46a7-9518-009213952978",
      "name": "Alice",
      "registeredAt": "2025-09-12T08:00:00Z"
    }
*/