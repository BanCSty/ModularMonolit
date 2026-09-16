using Microsoft.Extensions.Logging;
using Shared.Events;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Application.EventHandlers
{
    public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventHandler> _logger;
        private readonly IUserOrderSummaryRepository _userOrderSummaryRepository;
        private readonly UserUnitOfWork _unitOfWork;

        public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger, IUserOrderSummaryRepository userOrderSummaryRepository, UserUnitOfWork unitOfWork)
        {
            _logger = logger;
            _userOrderSummaryRepository = userOrderSummaryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(OrderCreatedEvent @event)
        {
            var summary = await _userOrderSummaryRepository.GetByUserIdAsync(@event.UserId);

            if (summary == null)
            {
                summary = new UserOrderSummary(@event.UserId);
                summary.UpdateWithNewOrder(@event.TotalAmount, @event.OccurredAt);
                await _userOrderSummaryRepository.AddAsync(summary);
            }
            else
            {
                summary.UpdateWithNewOrder(@event.TotalAmount, @event.OccurredAt);
                await _userOrderSummaryRepository.UpdateAsync(summary);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Updated order summary for user {UserId}: {TotalOrders} orders, {TotalSpent} total",
                @event.UserId, summary.TotalOrders, summary.TotalSpent);
        }
    }
}
