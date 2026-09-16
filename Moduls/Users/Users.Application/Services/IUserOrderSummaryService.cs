using Users.Domain.Entities;

namespace Users.Application.Services
{
    public interface IUserOrderSummaryService
    {
        Task<List<UserOrderSummary>> GetAllOrdersSummary(CancellationToken cancellation = default);
    }
}
