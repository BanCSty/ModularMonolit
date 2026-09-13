using Microsoft.Extensions.Logging;
using Shared.Events;

namespace Orders.Application.EventHandlers
{
    public class UserUpdatedEventHandler : IEventHandler<UserUpdatedEvent>
    {
        private readonly ILogger<UserUpdatedEventHandler> _logger;

        public UserUpdatedEventHandler(ILogger<UserUpdatedEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(UserUpdatedEvent @event)
        {
            //TODO
            _logger.LogInformation("⭐ UserUpdatedEventHandler called! UserId: {UserId}, Name: {Name}, Email: {Email}",
                @event.UserId, @event.Name, @event.Email);

            await Task.CompletedTask;
        }
    }
}
