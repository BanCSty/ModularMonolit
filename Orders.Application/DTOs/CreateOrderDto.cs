using Orders.Application.Services;

namespace Orders.Application.DTOs
{
    public record CreateOrderDto(int UserId, List<CreateOrderItemDto> Items);
}
