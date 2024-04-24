using System.Reflection;
using Application.Common.Interfaces;
using EasyCaching.InMemory;
using Infrastructure.Services.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddSingleton<ICacheService, CacheService>();

        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        return services;
    }
    
    public static IServiceCollection AddCustomCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEasyCaching(options =>
        {
            options.WithJson();

            if (string.Equals(configuration["EasyCaching:redisEnabled"], "true", StringComparison.InvariantCultureIgnoreCase))
            {
                options.UseRedis(configuration)
                    .WithJson();
            }
            else
            {
                options.UseInMemory(configuration);
            }
        });

        return services;
    }
}