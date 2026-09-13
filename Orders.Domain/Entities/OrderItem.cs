namespace Orders.Domain.Entities;

public class OrderItem
{
    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private OrderItem() { } // Для EF

    public OrderItem(string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));
        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than 0", nameof(unitPrice));

        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public decimal TotalPrice => Quantity * UnitPrice;
}