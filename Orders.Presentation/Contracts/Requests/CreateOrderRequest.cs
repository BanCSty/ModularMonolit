namespace Orders.Presentation.Contracts.Requests;

public record CreateOrderRequest(
    int UserId,
    List<CreateOrderItemRequest> Items
);

public record CreateOrderItemRequest(
    string ProductName,
    int Quantity,
    decimal UnitPrice
);