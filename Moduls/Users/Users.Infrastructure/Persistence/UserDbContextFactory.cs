using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Persistence;
using Shared.Infrastructure;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var dbPath = DatabasePathHelper.GetDatabasePath("users.db");

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new UserDbContext(optionsBuilder.Options);
    }
}