# Shared.Extensions

Методы расширения `IServiceCollection` и других типов для упрощения конфигурации.

## 🎯 Назначение

Убирает **бойлерплейт** из `Program.cs` и модулей, предоставляя удобные методы расширения.

## 📦 Содержимое

### `DbContextExtensions`

Методы для регистрации DbContext.

```csharp
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
```