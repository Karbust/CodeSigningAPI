using System.Net;
using System.Security.Claims;
using Application.Options;
using Application.Services.Settings;
using Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication;

public class TokenAuthenticationMiddleware(
    RequestDelegate next, 
    IOptions<SettingOptions> settingOptions,
    ILogger<TokenAuthenticationMiddleware> logger
)
{
    public async Task Invoke(HttpContext context, ISettingsService settingsService)
    {
        if (!settingOptions.Value.EnableAuthentication)
        {
            await next(context);
            return;
        }
        
        var ipAddress = context.Connection.RemoteIpAddress;
        
        if (settingOptions.Value is { EnableIPsWhitelist: true, BypassAuthenticationLoopback: true })
        {
            if (IPAddress.IsLoopback(ipAddress))
            {
                await next(context);
                return;
            }
        }

        if (!context.Request.Headers.TryGetValue("Authorization", out var tokenHeader))
        {
            throw new UnauthorizedAccessException();
        }
        
        var token = tokenHeader.FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException();
        }
            
        var authToken = await settingsService.IsAllowedToken(token);

        if (authToken is { Success: true, Data: not null })
        {
            var claims = new[]
            {
                new Claim(CustomClaimTypes.TokenId, authToken.Data.Id.ToString()),
                new Claim(CustomClaimTypes.TokenDescription, authToken.Data.Description)
            };
            var identity = new ClaimsIdentity(claims, "Token");
            var principal = new ClaimsPrincipal(identity);

            context.User = principal;

            await next(context);
            return;
        }

        throw new UnauthorizedAccessException();
    }
}
