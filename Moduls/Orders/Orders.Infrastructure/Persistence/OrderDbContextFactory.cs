using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace Orders.Infrastructure.Persistence
{
    public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            var dbPath = DatabasePathHelper.GetDatabasePath("orders.db");

            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new OrderDbContext(optionsBuilder.Options);
        }
    }
}
