using Microsoft.Extensions.Logging;
using Shared.Events;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Application.EventHandlers
{
    public class OrderDeletedEventHandler : IEventHandler<DeleteOrderEvent>
    {
        private readonly ILogger<OrderDeletedEventHandler> _logger;
        private readonly IUserOrderSummaryRepository _userOrderSummaryRepository;
        private readonly UserUnitOfWork _unitOfWork;

        public OrderDeletedEventHandler(ILogger<OrderDeletedEventHandler> logger, IUserOrderSummaryRepository userOrderSummaryRepository, UserUnitOfWork unitOfWork)
        {               
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userOrderSummaryRepository = userOrderSummaryRepository;
        }
        public async Task HandleAsync(DeleteOrderEvent @event)
        {
            var summary = await _userOrderSummaryRepository.GetByUserIdAsync(@event.UserId);

            if (summary != null)
            {

                summary.UpdateWithDeleteOrder(@event.TotalAmount);
                await _userOrderSummaryRepository.UpdateAsync(summary);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated order summary for user {UserId}: {TotalOrders} orders, {TotalSpent} total",
                    @event.UserId, summary.TotalOrders, summary.TotalSpent);
            }
            else
            {
                _logger.LogWarning(
                    "Order summary not found for user {UserId} when deleting order {OrderId}",
                    @event.UserId, @event.OrderId);
            }
        }
    }
}
