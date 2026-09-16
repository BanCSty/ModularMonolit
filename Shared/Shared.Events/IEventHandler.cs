namespace Shared.Events
{
    public interface IEventHandler<TEvent> where TEvent : class
    {
        Task HandleAsync(TEvent @event);
    }
}
