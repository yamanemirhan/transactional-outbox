using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Handlers;
using OrderService.Application.Messages;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetOrderByIdQueryHandler).Assembly));
        services.AddScoped<OutboxProcessor>();

        return services;
    }
}
