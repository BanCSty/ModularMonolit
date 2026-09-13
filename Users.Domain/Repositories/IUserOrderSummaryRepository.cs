using Users.Domain.Entities;

namespace Users.Domain.Repositories
{
    public interface IUserOrderSummaryRepository
    {
        Task<UserOrderSummary?> GetByUserIdAsync(int userId, CancellationToken cancellation = default);
        Task AddAsync(UserOrderSummary summary, CancellationToken cancellation = default);
        Task UpdateAsync(UserOrderSummary summary, CancellationToken cancellation = default);
        Task<List<UserOrderSummary>> GetAllUserOrderSummary(CancellationToken cancellation = default);
    }
}
