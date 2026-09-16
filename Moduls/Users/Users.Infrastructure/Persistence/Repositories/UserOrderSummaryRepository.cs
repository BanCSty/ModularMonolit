using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Infrastructure.Persistence.Repositories
{
    public class UserOrderSummaryRepository : IUserOrderSummaryRepository
    {
        private readonly UserDbContext _dbContext;

        public UserOrderSummaryRepository(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(UserOrderSummary summary, CancellationToken cancellation = default)
        {
            await _dbContext.UserOrderSummary.AddAsync(summary, cancellation);
        }

        public async Task<UserOrderSummary?> GetByUserIdAsync(int userId, CancellationToken cancellation = default)
        {
            return await _dbContext.UserOrderSummary.FirstOrDefaultAsync(x => x.UserId == userId, cancellation);
        }

        public async Task UpdateAsync(UserOrderSummary summary, CancellationToken cancellation = default)
        {
            var existingSummary = await _dbContext.UserOrderSummary
                .FirstOrDefaultAsync(x => x.UserId == summary.UserId, cancellation);

            if (existingSummary != null)
            {
                _dbContext.UserOrderSummary.Update(summary);
            }
        }

        public async Task<List<UserOrderSummary>> GetAllUserOrderSummary(CancellationToken cancellation = default)
        { 
            return await _dbContext.UserOrderSummary.AsNoTracking().ToListAsync(cancellation);
        }
    }
}
