namespace Orders.Application.DTOs
{
    public record CreateOrderItemDto(string ProductName, int Quantity, decimal UnitPrice);
}
