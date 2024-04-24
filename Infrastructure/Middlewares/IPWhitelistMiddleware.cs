using System.Net;
using Application.Options;
using Application.Services.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Middlewares;

public class IPWhitelistMiddleware(
    RequestDelegate next,
    IOptions<SettingOptions> settingOptions,
    ILogger<IPWhitelistMiddleware> logger
)
{
    public async Task Invoke(HttpContext context, ISettingsService settingsService)
    {
        if (!settingOptions.Value.EnableIPsWhitelist)
        {
            await next(context);
            return;
        }
        
        var ipAddress = context.Connection.RemoteIpAddress;
        
        if (IPNetwork2.TryParse(ipAddress.ToString(), out var ipNetwork2))
        {
            if (IPAddress.IsLoopback(ipAddress))
            {
                await next(context);
                return;
            }
            
            if (ipNetwork2.IsIANAReserved())
            {
                var settingsToRanges = new Dictionary<Func<bool>, IPNetwork2?>
                {
                    { () => settingOptions.Value.AllowCGNatIPs, IPNetwork2.Parse("100.64.0.0/10") },
                    { () => settingOptions.Value.AllowAllPrivateIanaReservedIPs, null },
                    { () => settingOptions.Value.AllowClassAReservedIPs, IPNetwork2.IANA_ABLK_RESERVED1 },
                    { () => settingOptions.Value.AllowClassBReservedIPs, IPNetwork2.IANA_BBLK_RESERVED1 },
                    { () => settingOptions.Value.AllowClassCReservedIPs, IPNetwork2.IANA_CBLK_RESERVED1 }
                };

                if (settingsToRanges.Any(settingToRange => settingToRange.Key() && (settingToRange.Value == null || settingToRange.Value.Contains(ipNetwork2))))
                {
                    await next(context);
                    return;
                }
            }
        
            var isIpAllowed = await settingsService.IsAllowedIP(ipAddress.ToString());

            if (isIpAllowed is { Success: true, Data.IsAllowed: true })
            {
                await next(context);
                return;
            }
            
            logger.LogWarning("Request from Remote IP address: {RemoteIp} is forbidden.", ipAddress);
            throw new UnauthorizedAccessException("Request from Remote IP address is forbidden.");
        }
    }
}