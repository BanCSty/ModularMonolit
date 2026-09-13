namespace Users.Domain.Entities
{
    public class UserOrderSummary
    {
        public int UserId { get; private set; }
        public int TotalOrders { get; private set; }
        public decimal TotalSpent { get; private set; }
        public DateTime? LastOrderDate { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private UserOrderSummary() { }

        public UserOrderSummary(int userId)
        {
            UserId = userId;
            TotalOrders = 0;
            TotalSpent = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateWithNewOrder(decimal amount, DateTime orderDate)
        {
            TotalOrders++;
            TotalSpent += amount;
            LastOrderDate = orderDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateWithDeleteOrder(decimal amount)
        {
            TotalOrders = Math.Max(0, TotalOrders - 1);  
            TotalSpent = Math.Max(0, TotalSpent - amount);
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateWithChangedOrder(decimal amountDifference)
        {
            TotalSpent += amountDifference;
            TotalSpent = Math.Max(0, TotalSpent);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
