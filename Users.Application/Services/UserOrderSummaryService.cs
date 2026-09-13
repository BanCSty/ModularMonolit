using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Services
{
    public class UserOrderSummaryService : IUserOrderSummaryService
    {
        private readonly IUserOrderSummaryRepository _userOrderSummaryRepository;

        public UserOrderSummaryService(IUserOrderSummaryRepository userOrderSummaryRepository)
        {
            _userOrderSummaryRepository = userOrderSummaryRepository;
        }
        public async Task<List<UserOrderSummary>> GetAllOrdersSummary(CancellationToken cancellation = default)
        {
            return await _userOrderSummaryRepository.GetAllUserOrderSummary(cancellation);
        }
    }
}
