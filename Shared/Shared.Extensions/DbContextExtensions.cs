using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Shared.Extensions;

public static class DbContextExtensions
{
    public static IServiceCollection AddDatabase<TContext>(
    this IServiceCollection services,
    IConfiguration configuration,
    string connectionStringName,
    string databaseName) where TContext : DbContext
    {
        var dbPath = DatabasePathHelper.GetDatabasePath(databaseName);

        services.AddDbContext<TContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        return services;
    }
}