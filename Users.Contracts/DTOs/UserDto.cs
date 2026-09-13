namespace Users.Contracts.DTOs;

public record UserDto(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAt
);