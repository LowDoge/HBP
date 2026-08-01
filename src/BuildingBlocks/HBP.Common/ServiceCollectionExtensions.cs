using Microsoft.Extensions.DependencyInjection;

namespace HBP.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHbpCommon(this IServiceCollection services)
    {
        services.AddTransient<IClock, SystemClock>();

        return services;
    }
}
