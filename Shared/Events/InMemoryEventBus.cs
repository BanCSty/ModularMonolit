using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Events;
public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Type>> _handlerTypes = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(IServiceScopeFactory scopeFactory, ILogger<InMemoryEventBus> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void Subscribe<TEvent, THandler>()
    where TEvent : class
    where THandler : IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        if (!_handlerTypes.ContainsKey(eventType))
            _handlerTypes[eventType] = new List<Type>();

        if (!_handlerTypes[eventType].Contains(handlerType))
        {
            _handlerTypes[eventType].Add(handlerType);
            _logger.LogInformation("Subscribed {HandlerType} to {EventType}",
                handlerType.Name, eventType.Name);
        }
        else
        {
            _logger.LogWarning("Handler {HandlerType} already subscribed to {EventType}",
                handlerType.Name, eventType.Name);
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class
    {
        await PublishAsync(@event, typeof(TEvent), ct);
    }

    public async Task PublishAsync(object @event, Type eventType, CancellationToken ct = default)
    {
        _logger.LogInformation("Publishing event {EventType}", eventType.FullName);

        //Проверяем, есть ли обработчики для этого типа события
        if (!_handlerTypes.ContainsKey(eventType))
        {
            //Если нет - выходим
            _logger.LogWarning("No handlers registered for event {EventType}. Available handlers: {Handlers}",
                eventType.FullName,
                string.Join(", ", _handlerTypes.Keys.Select(k => k.Name)));
            return;
        }

        //Получаем список обработчиков -> пример UserUpdatedEvent
        //_handlerTypes[typeof(UserUpdatedEvent)] = [typeof(UserUpdatedEventHandler)]
        foreach (var handlerType in _handlerTypes[eventType])
        {
            using var scope = _scopeFactory.CreateScope();

            //Получаем экземпляр обработчика из DI
            //handler = new UserUpdatedEventHandler
            var handler = scope.ServiceProvider.GetService(handlerType);

            if (handler == null)
            {
                _logger.LogError("Handler {HandlerType} not registered in DI", handlerType.Name);
                continue;
            }

            try
            {
                // Находим метод HandleAsync через reflection
                //handleMethod = UserUpdatedEventHandler.HandleAsync(UserUpdatedEvent)
                var handleMethod = handlerType.GetMethod("HandleAsync");

                if (handleMethod != null)
                {
                    _logger.LogInformation("Invoking handler {HandlerType} for event {EventType}",
                        handlerType.Name, eventType.Name);

                    //await handler.HandleAsync(userUpdatedEvent)
                    await (Task)handleMethod.Invoke(handler, new[] { @event });

                    _logger.LogInformation("Handler {HandlerType} completed successfully",
                        handlerType.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling {EventType} in {HandlerType}",
                    eventType.Name, handlerType.Name);
            }
        }
    }
}