using Infrastructure.Authentication;

namespace CodeSigningApi.Extensions;

public class TokenAuthenticationMiddlewareExtensions
{
    public void Configure(IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseMiddleware<TokenAuthenticationMiddleware>();
    }
}