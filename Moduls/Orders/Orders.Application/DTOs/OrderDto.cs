using Orders.Domain.Entities;

namespace Orders.Application.DTOs;

public class OrderDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public decimal Total { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
}

public class OrderItemDto
{
    public int Id { get; init; }
    public string ProductName { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
}