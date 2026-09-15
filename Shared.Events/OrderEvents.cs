namespace Shared.Events;

public record OrderCreatedEvent(
    int UserId,
    decimal TotalAmount,
    string Status,
    DateTime OccurredAt
);

public record OrderStatusChangedEvent(
    int OrderId,
    string OldStatus,
    string NewStatus,
    DateTime OccurredAt
);

public record DeleteOrderEvent(
    int OrderId,
    int UserId,
    decimal TotalAmount
);

public record UpdateOrderEvent(
    int OrderId,
    int UserId,
    decimal OldTotalAmount,
    decimal NewTotalAmount,
    DateTime OccurredAt
);