using System.Reflection;
using Application.Services.Settings;
using Application.Services.Signing;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddCustomServices();
        
        return services;
    }

    private static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services
            .AddScoped<ISigningService, SigningService>()
            .AddScoped<ISettingsService, SettingsService>();

        return services;
    }
}