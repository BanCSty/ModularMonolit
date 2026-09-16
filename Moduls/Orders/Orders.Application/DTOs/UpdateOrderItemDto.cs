using System.ComponentModel.DataAnnotations;

namespace Orders.Application.DTOs
{
    public record UpdateOrderItemDto
    {
        public string ProductName { get; init; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; init; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal UnitPrice { get; init; }
    }
}
