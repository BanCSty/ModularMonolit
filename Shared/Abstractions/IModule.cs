using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Events;

namespace Shared.Abstractions
{
    public interface IModule 
    {
        string Name { get; }
        IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration);
        IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);

        /// <summary>
        /// Настройка подписок
        /// </summary>
        /// <param name="eventBus"></param>
        void ConfigureEventSubscriptions(IEventBus eventBus) { }
    }
}
