namespace Shared.Events;
public interface IEventBus
{
    void Subscribe<TEvent, THandler>()
        where TEvent : class
        where THandler : IEventHandler<TEvent>;

    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class;

    Task PublishAsync(object @event, Type eventType, CancellationToken ct = default);
}
