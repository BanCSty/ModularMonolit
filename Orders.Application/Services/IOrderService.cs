using Orders.Application.DTOs;
using Shared.Outbox;

namespace Orders.Application.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto, CancellationToken cancellationToken = default);
    Task UpdateOrderStatusAsync(int id, UpdateOrderStatusDto updateStatusDto, CancellationToken cancellationToken = default);
    Task DeleteOrderAsync(int id, CancellationToken cancellationToken = default);
    Task<List<OutboxMessage>> GetOutboxMessageAsync(CancellationToken cancellation = default);
}