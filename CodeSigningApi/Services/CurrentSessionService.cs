using System.Net;
using System.Security.Claims;
using Application.Common.Interfaces;
using Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CodeSigningApi.Services;

public class CurrentSessionService(
    IHttpContextAccessor httpContextAccessor,
    IOptions<IdentityOptions> identityOptions
) : ICurrentSessionService
{
    private readonly IOptions<IdentityOptions> _identityOptions = identityOptions;

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public int? TokenId
    {
        get
        {
            if (!IsAuthenticated)
            {
                return null;
            }

            var claimValue = httpContextAccessor.HttpContext?.User.FindFirstValue(CustomClaimTypes.TokenId);

            if (int.TryParse(claimValue, out int tempValue))
                return tempValue;

            return null;
        }
    }

    public string? TokenDescription => !IsAuthenticated ? null : httpContextAccessor.HttpContext?.User.FindFirstValue(CustomClaimTypes.TokenDescription);

    public IPAddress? IpAddress => httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;
    
}