using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Shared.Abstractions
{
    public abstract class BaseUnitOfWork<TDbContext> : IUnitOfWork where TDbContext : DbContext
    {
        protected readonly TDbContext DbContext;
        private IDbContextTransaction? _transaction;

        protected BaseUnitOfWork(TDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        public virtual async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
        {
            await BeginTransactionAsync(cancellationToken);
            try
            {
                await operation();
                await CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public virtual async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<Task<TResult>> operation,
            CancellationToken cancellationToken = default)
        {
            await BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation();
                await CommitTransactionAsync(cancellationToken);
                return result;
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public virtual int GetPendingChangesCount()
        {
            return DbContext.ChangeTracker.Entries()
                .Count(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted);
        }

        public virtual int GetDbContextHashCode()
        {
            return DbContext.GetHashCode();
        }

        public virtual void Dispose()
        {
            _transaction?.Dispose();
            DbContext.Dispose();
        }
    }
}
