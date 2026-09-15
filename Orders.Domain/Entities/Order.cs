namespace Orders.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    private Order(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId must be greater than 0", nameof(userId));

        UserId = userId;
        Total = 0;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Order CreatePending(int userId)
    {
        return new Order(userId);
    }

    public void SetTotal(decimal total)
    {
        if (total <= 0)
            throw new ArgumentException("Total must be greater than 0", nameof(total));

        Total = total;
    }

    public void Update(decimal total)
    {
        if (total <= 0)
            throw new ArgumentException("Total must be greater than 0", nameof(total));

        Total = total;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(OrderItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _items.Add(item);
        RecalculateTotal();
    }

    public void RemoveItem(OrderItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _items.Remove(item);
        RecalculateTotal();
    }

    // Пересчёт общей суммы
    private void RecalculateTotal()
    {
        Total = _items.Sum(i => i.TotalPrice);
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal CalculateTotal()
    {
        return _items.Sum(i => i.Quantity * i.UnitPrice);
    }

    public void UpdateTotal(decimal total)
    {
        Total = total;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearItems()
    {
        _items.Clear();
    }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Updated
}