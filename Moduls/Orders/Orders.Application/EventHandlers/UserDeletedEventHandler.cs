using Microsoft.Extensions.Logging;
using Shared.Events;

namespace Orders.Application.EventHandlers
{
    public class UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
    {
        private readonly ILogger<UserDeletedEventHandler> _logger;

        public UserDeletedEventHandler(ILogger<UserDeletedEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(UserDeletedEvent @event)
        {
            //TODO ...
            _logger.LogInformation("User delete: {UserId}, {Name}", @event.UserId);

            await Task.CompletedTask;
        }
    }
}
