using HBP.Hotel.Application.Abstractions;
using HBP.Hotel.Infrastructure.Caching;
using HBP.Hotel.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HBP.Hotel.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHotelInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string redisConnectionName
    )
    {
        services.AddScoped<IHotelRepository, HotelRepository>();

        services.AddScoped<IHotelCache, HotelCache>();
        services.AddStackExchangeRedisCache(options =>
            options.Configuration =
                configuration.GetConnectionString(redisConnectionName)
                ?? throw new InvalidOperationException(
                    $"Connection '{redisConnectionName}' is not configured."
                )
        );

        return services;
    }
}
