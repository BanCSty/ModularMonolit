using Microsoft.Extensions.Logging;
using Shared.Events;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Application.EventHandlers
{
    public class OrderUpdatedEventHandler : IEventHandler<UpdateOrderEvent>
    {
        private readonly IUserOrderSummaryRepository _userOrderSummaryRepository;
        private readonly ILogger<OrderUpdatedEventHandler> _logger;
        private readonly UserUnitOfWork _unitOfWork;

        public OrderUpdatedEventHandler(ILogger<OrderUpdatedEventHandler> logger, IUserOrderSummaryRepository userOrderSummaryRepository, UserUnitOfWork unitOfWork)
        {
            _logger = logger;
            _userOrderSummaryRepository = userOrderSummaryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(UpdateOrderEvent @event) 
        {
            var summary = await _userOrderSummaryRepository.GetByUserIdAsync(@event.UserId);

            if (summary != null)
            {
                var totalDifference = @event.NewTotalAmount - @event.OldTotalAmount;

                summary.UpdateWithChangedOrder(totalDifference);
                await _userOrderSummaryRepository.UpdateAsync(summary);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated order summary for user {UserId}: {TotalOrders} orders, old total {OldTotalSpent}, new total{NewTotalSpent}",
                    @event.UserId, summary.TotalOrders, @event.OldTotalAmount, @event.NewTotalAmount);
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
