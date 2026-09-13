using Microsoft.EntityFrameworkCore;

namespace Shared.Outbox
{
    public interface IOutboxService<TContext> where TContext : DbContext
    {
        Task AddEventAsync<TEvent>(TEvent @event) where TEvent : class;
        int GetDbContextHashCode();
    }
}
