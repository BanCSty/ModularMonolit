using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions;
using Shared.Events;
using Shared.Infrastructure;
using Shared.Outbox;
using Users.Application.EventHandlers;
using Users.Application.Services;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence.Repositories;
using Users.Infrastructure.Persistence;
using Users.Presentation.Contracts.Requests;
using Users.Presentation.Filters;
using Users.Presentation.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;

public class UsersModule : IModule
{
    public string Name => "Users";

    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        // Presentation
        services.AddControllers()
            .AddApplicationPart(typeof(UsersModule).Assembly);

        services.AddScoped<UserExceptionFilter>();
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();

        // Application
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserOrderSummaryService, UserOrderSummaryService>();

        // Infrastructure
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserOrderSummaryRepository, UserOrderSummaryRepository>();

        services.AddScoped<UserUnitOfWork>();

        var dbPath = DatabasePathHelper.GetDatabasePath("users.db");

        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IOutboxService<UserDbContext>, OutboxService<UserDbContext>>();

        services.AddHostedService<OutboxPublisher<UserDbContext>>();
        services.AddScoped<OrderCreatedEventHandler>();
        services.AddScoped<OrderDeletedEventHandler>();
        services.AddScoped<OrderUpdatedEventHandler>();
        return services;
    }

    public void ConfigureEventSubscriptions(IEventBus eventBus)
    {
        eventBus.Subscribe<OrderCreatedEvent, OrderCreatedEventHandler>();
        eventBus.Subscribe<DeleteOrderEvent, OrderDeletedEventHandler>();
        eventBus.Subscribe<UpdateOrderEvent, OrderUpdatedEventHandler>();
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        return endpoints;
    }
}