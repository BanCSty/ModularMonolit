using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shared.Outbox;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Outbox
{
    public class UserOutboxService : IOutboxService<UserDbContext>
    {
        private readonly UserDbContext _dbContext;

        public UserOutboxService(UserDbContext dbContext)
        {
            _dbContext = dbContext;
            Console.WriteLine($"[UserOutboxService] Constructor - DbContext HashCode: {_dbContext.GetHashCode()}");
        }

        public async Task AddEventAsync<TEvent>(TEvent @event) where TEvent : class
        {
            var message = new OutboxMessage(
                type: typeof(TEvent).FullName!,
                payload: JsonSerializer.Serialize(@event)
            );

            Console.WriteLine($"[UserOutboxService] Adding to DbContext: {_dbContext.GetHashCode()}");

            await _dbContext.Set<OutboxMessage>().AddAsync(message);

            var pendingCount = _dbContext.ChangeTracker.Entries()
                .Count(e => e.State == EntityState.Added);
            Console.WriteLine($"[UserOutboxService] Pending changes: {pendingCount}");
        }

        public int GetDbContextHashCode()
        {
            return _dbContext.GetHashCode();
        }
    }
}
