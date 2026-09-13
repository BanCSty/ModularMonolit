namespace Shared.Events;

public record UserCreatedEvent(
    int UserId,
    string Name,
    string Email,
    DateTime OccurredAt
);

public record UserUpdatedEvent(
    int UserId,
    string Name,
    string Email,
    DateTime OccurredAt
);

public record UserDeletedEvent(
    int UserId,
    DateTime OccurredAt
);