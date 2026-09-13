
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.EventHandlers;
using Orders.Application.Services;
using Orders.Domain.Repositories;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Persistence.Repositories;
using Orders.Presentation.Contracts.Requests;
using Orders.Presentation.Filters;
using Orders.Presentation.Validators;
using Shared.Abstractions;
using Shared.Events;
using Shared.Extensions;
using Shared.Outbox;

namespace Orders.Presentation;

public class OrdersModule : IModule
{
    public string Name => "Orders";

    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        // Presentation
        services.AddControllers()
            .AddApplicationPart(typeof(OrdersModule).Assembly);

        services.AddScoped<OrderExceptionFilter>();
        services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
        services.AddScoped<IValidator<UpdateOrderStatusRequest>, UpdateOrderStatusRequestValidator>();

        // Application
        services.AddScoped<IOrderService, OrderService>();

        // Infrastructure
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<OrderUnitOfWork>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(OrdersModule).Assembly);
        });

        services.AddDatabase<OrderDbContext>(configuration, "OrdersDatabase", "orders.db");

        services.AddScoped<IOutboxService<OrderDbContext>, OutboxService<OrderDbContext>>();


        services.AddHostedService<OutboxPublisher<OrderDbContext>>();
        services.AddScoped<UserCreatedEventHandler>();
        services.AddScoped<UserUpdatedEventHandler>();
        return services;
    }

    public void ConfigureEventSubscriptions(IEventBus eventBus)
    {
        eventBus.Subscribe<UserCreatedEvent, UserCreatedEventHandler>();
        eventBus.Subscribe<UserUpdatedEvent, UserUpdatedEventHandler>();
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        return endpoints;
    }
}