using Infrastructure.Middlewares;

namespace CodeSigningApi.Extensions;

public static class IPWhitelistMiddlewareExtensions
{
    public static IApplicationBuilder UseIPWhitelist(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IPWhitelistMiddleware>();
    }
}