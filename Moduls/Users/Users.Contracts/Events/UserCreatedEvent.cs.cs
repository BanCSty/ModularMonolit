namespace Users.Contracts.Events;

public record UserCreatedEvent(
    int UserId,
    string Name,
    string Email,
    DateTime CreatedAt
);