using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Shared.Outbox;

public class OutboxService<TContext> : IOutboxService<TContext> where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly ILogger<OutboxService<TContext>> _logger;

    public OutboxService(TContext dbContext, ILogger<OutboxService<TContext>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task AddEventAsync<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = typeof(TEvent); //Example UserUpdatedEvent

        //Сохраняем полное имя типа: например "Shared.Events.UserUpdatedEvent, Shared"
        //Сохраняем полное имя с assembly для правильной десериализации
        var typeName = $"{eventType.FullName}, {eventType.Assembly.GetName().Name}";

        var message = new OutboxMessage(
            type: typeName,
            payload: JsonSerializer.Serialize(@event)  // "{\"UserId\":1,\"Name\":\"John\",...}"
        );

        _logger.LogInformation("Adding event {EventType} to outbox", typeName);
        //Добавляем в DbContext (но еще не сохраняем в БД
        await _dbContext.Set<OutboxMessage>().AddAsync(message);
        // SaveChanges будет вызван в UnitOfWork
    }

    public int GetDbContextHashCode()
    {
        return _dbContext.GetHashCode();
    }
}