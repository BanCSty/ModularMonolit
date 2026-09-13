using Microsoft.Extensions.Logging;
using Shared.Events;

namespace Orders.Application.EventHandlers;

public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserCreatedEvent @event)
    {
        _logger.LogInformation("User created: {UserId}, {Name}", @event.UserId, @event.Name);

        //TODO ...
        //Создание корзины

        await Task.CompletedTask;
    }
}