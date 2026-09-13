using Shared.Abstractions;

namespace Orders.Infrastructure.Persistence
{
    public class OrderUnitOfWork : BaseUnitOfWork<OrderDbContext>
    {
        public OrderUnitOfWork(OrderDbContext orderDbContext) : base(orderDbContext) { }
    }
}
