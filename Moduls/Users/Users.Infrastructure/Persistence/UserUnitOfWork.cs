using Shared.Abstractions;

namespace Users.Infrastructure.Persistence
{
    public class UserUnitOfWork : BaseUnitOfWork<UserDbContext>
    {
        public UserUnitOfWork(UserDbContext userDbContext) : base(userDbContext) { }
    }
}
