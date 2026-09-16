using System.ComponentModel.DataAnnotations;
using Orders.Application.Services;

namespace Orders.Application.DTOs
{
    public record UpdateOrderDto
    {
        public List<UpdateOrderItemDto> Items { get; init; } = new();

        [MinLength(1, ErrorMessage = "Order must have at least one item")]
        public IReadOnlyList<UpdateOrderItemDto> ValidatedItems => Items;
    }
}
