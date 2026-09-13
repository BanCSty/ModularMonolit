namespace Orders.Presentation.Contracts.Responses;

public record OrderResponse(
    int Id,
    int UserId,
    decimal Total,
    string Status,
    DateTime CreatedAt,
    List<OrderItemResponse> Items
);

public record OrderItemResponse(
    int Id,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);