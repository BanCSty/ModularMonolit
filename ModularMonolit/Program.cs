using Shared.Abstractions;
using Orders.Presentation;
using Orders.Infrastructure.Persistence;
using Users.Infrastructure.Persistence;
using Shared.Events;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = DatabasePathHelper.GetDataDirectory();
AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
var modules = new List<IModule>
{
    new UsersModule(),
    new OrdersModule()
};

foreach (var module in modules)
{
    module.RegisterModule(builder.Services, builder.Configuration);
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Автоматическое создание БД при запуске
using (var scope = app.Services.CreateScope())
{
    try
    {
        var userDbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await userDbContext.Database.MigrateAsync();
        app.Logger.LogInformation("Users database migrated successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error migrating Users database");
    }

    try
    {
        var orderDbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await orderDbContext.Database.MigrateAsync();
        app.Logger.LogInformation("Orders database migrated successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error migrating Orders database");
    }
}
var eventBus = app.Services.GetRequiredService<IEventBus>();

foreach (var module in modules)
{
    // Настраиваем подписки для каждого модуля
    module.ConfigureEventSubscriptions(eventBus);

    // Маппинг эндпоинтов модулей
    module.MapEndpoints(app);

    // Логирование зарегистрированных модулей
    app.Logger.LogInformation("Module {ModuleName} registered", module.Name);
}

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FiltersLesson API v1");
        c.RoutePrefix = string.Empty;
    });

    app.UseDeveloperExceptionPage();
}
// Disable HTTPS with Docker
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
