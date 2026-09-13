namespace Users.Contracts.Events;

public record UserDeletedEvent(
    int UserId,
    DateTime DeletedAt
);