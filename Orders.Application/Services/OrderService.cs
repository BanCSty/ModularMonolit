using Microsoft.Extensions.Logging;
using Orders.Application.DTOs;
using Orders.Application.Exceptions;
using Orders.Domain.Entities;
using Orders.Domain.Repositories;
using Orders.Infrastructure.Persistence;
using Shared.Events;
using Shared.Outbox;

namespace Orders.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxService<OrderDbContext> _outboxService;
    private readonly OrderUnitOfWork _orderUnitOfWork;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IOutboxService<OrderDbContext> outboxService,
        OrderUnitOfWork orderUnitOfWork
,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _outboxService = outboxService;
        _orderUnitOfWork = orderUnitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
            throw new OrderNotFoundException(id);

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId, cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, CancellationToken cancellationToken = default)
    {

        // Создаём заказ с временной суммой 0, потом пересчитаем
        var order = Order.CreatePending(createOrderDto.UserId);

        // Добавляем items к заказу
        foreach (var item in createOrderDto.Items)
        {
            order.AddItem(new OrderItem(item.ProductName, item.Quantity, item.UnitPrice));
        }

        try
        {
            await _orderUnitOfWork.BeginTransactionAsync(cancellationToken);

            var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);

            await _outboxService.AddEventAsync(new OrderCreatedEvent(
                    createdOrder.UserId,
                    createdOrder.Total,
                    createdOrder.Status.ToString(),
                    createdOrder.CreatedAt
            ));

            await _orderUnitOfWork.SaveChangesAsync(cancellationToken);
            await _orderUnitOfWork.CommitTransactionAsync(cancellationToken);
            return MapToDto(createdOrder);
        }
        catch (Exception)
        {
            await _orderUnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }     
    }

    public async Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto, CancellationToken cancellationToken = default)
    {
        try
        {
            await _orderUnitOfWork.BeginTransactionAsync(cancellationToken);
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
            if (order is null)
                throw new OrderNotFoundException(id);

            //Сохраняем старую сумму заказа
            var oldOrderTotal = order.Total;

            // Очищаем старые items
            order.ClearItems();

            // Добавляем новые items из DTO
            foreach (var item in updateOrderDto.Items)
            {
                order.AddItem(new OrderItem(
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice
                ));
            }

            order.Update(order.CalculateTotal());


            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _outboxService.AddEventAsync(new UpdateOrderEvent(
                    order.Id,
                    order.UserId,
                    oldOrderTotal,
                    order.Total,
                    DateTime.UtcNow
                ));

            await _orderUnitOfWork.SaveChangesAsync(cancellationToken);
            await _orderUnitOfWork.CommitTransactionAsync(cancellationToken);
            return MapToDto(order);
        }
        catch (Exception ex)
        {
            await _orderUnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }     
    }

    public async Task UpdateOrderStatusAsync(int id, UpdateOrderStatusDto updateStatusDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
            if (order is null)
                throw new OrderNotFoundException(id);

            if (!Enum.TryParse<OrderStatus>(updateStatusDto.Status, true, out var newStatus))
                throw new ArgumentException($"Invalid order status: {updateStatusDto.Status}");

            order.UpdateStatus(newStatus);

            await _orderUnitOfWork.BeginTransactionAsync(cancellationToken);
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _outboxService.AddEventAsync(new OrderStatusChangedEvent
                (
                    order.Id,
                    order.Status.ToString(),
                    updateStatusDto.Status,
                    DateTime.Now
                ));
            await _orderUnitOfWork.SaveChangesAsync(cancellationToken);
            await _orderUnitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await _orderUnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        
    }

    public async Task DeleteOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            if (order == null)
                throw new OrderNotFoundException(id);

            _logger.LogInformation($"Repository DbContext: {_orderRepository.GetDbContextHashCode()}");
            _logger.LogInformation($"OutboxService DbContext: {_outboxService.GetDbContextHashCode()}");
            _logger.LogInformation($"UnitOfWork DbContext: {_orderUnitOfWork.GetDbContextHashCode()}");

            await _orderUnitOfWork.BeginTransactionAsync(cancellationToken);
            await _orderRepository.DeleteAsync(id, cancellationToken);
            await _outboxService.AddEventAsync(new DeleteOrderEvent
                (
                    order.Id,
                    order.UserId,
                    order.Total
                ));

            await _orderUnitOfWork.SaveChangesAsync(cancellationToken);
            await _orderUnitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await _orderUnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }        
    }

    public async Task<List<OutboxMessage>> GetOutboxMessageAsync(CancellationToken cancellation = default)
    {
        return await _orderRepository.GetOutboxMessageAsync(cancellation);
    }

    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        UserId = order.UserId,
        Total = order.Total,
        Status = order.Status,
        CreatedAt = order.CreatedAt,
        Items = order.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.TotalPrice
        }).ToList()
    };
}