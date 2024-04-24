using Application.Services.Settings;

namespace CodeSigningApi.Extensions;

public static class CacheWebApplicationCollectionExtensions
{
    public static async Task<WebApplication> ApplyCache(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();

        await settingsService.GetAuthTokens();
        await settingsService.GetAllowedIPs();
        
        return app;
    }
}