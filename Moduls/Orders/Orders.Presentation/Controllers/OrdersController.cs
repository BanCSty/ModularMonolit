using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orders.Application.DTOs;
using Orders.Application.Exceptions;
using Orders.Application.Services;
using Orders.Presentation.Contracts.Requests;
using Orders.Presentation.Contracts.Responses;
using Orders.Presentation.Filters;

namespace Orders.Presentation.Controllers;

[ApiController]
[Route("api/orders")]
[ServiceFilter(typeof(OrderExceptionFilter))]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderRequest> _createOrderValidator;
    private readonly IValidator<UpdateOrderStatusRequest> _updateOrderStatusValidator;
    private readonly ILogger<OrdersController> _logger;


    public OrdersController(
        IOrderService orderService,
        IValidator<CreateOrderRequest> createOrderValidator,
        IValidator<UpdateOrderStatusRequest> updateOrderStatusValidator,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _createOrderValidator = createOrderValidator;
        _updateOrderStatusValidator = updateOrderStatusValidator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAllOrders(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllOrdersAsync(cancellationToken);
        var response = orders.Select(MapToResponse);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(int id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        return Ok(MapToResponse(order));
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersByUserId(int userId, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetOrdersByUserIdAsync(userId, cancellationToken);
        var response = orders.Select(MapToResponse);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createOrderValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var createOrderDto = new CreateOrderDto(
            request.UserId,
            request.Items.Select(i => new CreateOrderItemDto(i.ProductName, i.Quantity, i.UnitPrice)).ToList()
        );

        var order = await _orderService.CreateOrderAsync(createOrderDto, cancellationToken);
        var response = MapToResponse(order);

        return Created($"/api/orders/{response.Id}", response);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        // Валидация
        var validationResult = await _updateOrderStatusValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var updateStatusDto = new UpdateOrderStatusDto(request.Status);
        await _orderService.UpdateOrderStatusAsync(id, updateStatusDto, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> UpdateOrder(
        int id,
        [FromBody] UpdateOrderDto updateOrderDto,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating order {OrderId}", id);

            var updatedOrder = await _orderService.UpdateOrderAsync(
                id,
                updateOrderDto,
                cancellationToken);

            return Ok(updatedOrder);
        }
        catch (OrderNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order {OrderId} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the order" });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOrder(int id, CancellationToken cancellationToken)
    {
        await _orderService.DeleteOrderAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("GetOutboxMessages")]
    public async Task<IActionResult> GetOutboxMessages(CancellationToken cancellation)
    {
        return Ok(await _orderService.GetOutboxMessageAsync(cancellation));
    }

    private static OrderResponse MapToResponse(OrderDto order) => new(
        order.Id,
        order.UserId,
        order.Total,
        order.Status.ToString(),
        order.CreatedAt,
        order.Items.Select(i => new OrderItemResponse(
            i.Id,
            i.ProductName,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice
        )).ToList()
    );
}