using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Events;

namespace Shared.Outbox;

public class OutboxPublisher<TContext> : BackgroundService where TContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OutboxPublisher<TContext>> _logger;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IEventBus eventBus,
        ILogger<OutboxPublisher<TContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxPublisher started for {ContextType}", typeof(TContext).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

                var messages = await dbContext.Set<OutboxMessage>()
                    .Where(m => m.PublishedAt == null && m.RetryCount < 5)
                    .OrderBy(m => m.CreatedAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                _logger.LogInformation("Found {Count} unpublished messages in {ContextType}",
                    messages.Count, typeof(TContext).Name);

                foreach (var message in messages)
                {
                    try
                    {
                        _logger.LogInformation("Processing message {MessageId}, Type: {MessageType}",
                            message.Id, message.Type);

                        //Определяем тип события
                        //eventType = например typeof(UserUpdatedEvent)
                        var eventType = Type.GetType(message.Type);

                        if (eventType == null)
                        {
                            _logger.LogError("Unknown event type: {EventType}", message.Type);
                            message.MarkAsFailed($"Unknown event type: {message.Type}");
                            continue;
                        }

                        //Десериализуем событие
                        //@event = например new UserUpdatedEvent { UserId = 1, Name = "John", ... }
                        var @event = JsonSerializer.Deserialize(message.Payload, eventType);

                        if (@event == null)
                        {
                            _logger.LogError("Failed to deserialize event: {EventType}", message.Type);
                            message.MarkAsFailed("Failed to deserialize event");
                            continue;
                        }

                        _logger.LogInformation("Publishing event of type {EventType} to EventBus",
                            eventType.FullName);

                        await _eventBus.PublishAsync(@event, eventType, stoppingToken);

                        message.MarkAsPublished();

                        _logger.LogInformation("Successfully published outbox message {MessageId}",
                            message.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error publishing outbox message {MessageId}", message.Id);
                        message.MarkAsFailed(ex.Message);
                    }
                }

                if (messages.Any())
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Saved outbox changes");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxPublisher error for {ContextType}", typeof(TContext).Name);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}