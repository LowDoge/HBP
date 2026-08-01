using System.Reflection;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;

namespace HBP.Hotel.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHotelApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                Assembly.GetExecutingAssembly()
            ).NotificationPublisher = new ForeachAwaitPublisher()
        );

        return services;
    }
}
