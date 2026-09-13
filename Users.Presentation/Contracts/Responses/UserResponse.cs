namespace Users.Presentation.Contracts.Responses;

public record UserResponse(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAt
);