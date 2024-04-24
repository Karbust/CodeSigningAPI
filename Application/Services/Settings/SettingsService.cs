using System.Net;
using System.Text.RegularExpressions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Utils;
using Application.Services.Settings.Models.AuthTokens;
using Application.Services.Settings.Models.IPs;
using Application.Services.Signing;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Settings;

public class SettingsService(
    ILogger<SigningService> logger,
    IApplicationDbContext applicationDbContext,
    ICurrentSessionService currentSessionService,
    ICacheService cacheService,
    IMapper mapper
) : ISettingsService
{
    public async Task<Result<List<AuthTokens>>> GetAuthTokens()
    {
        var result = await cacheService.GetAsync(CacheKeys.AuthTokens.All, () =>
        {
            return applicationDbContext.AuthTokens
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => !c.IsRevoked && c.Id != Generic.System.TokenId)
                .ToListAsync();
        }, TimeSpan.FromDays(365));

        return new Result<List<AuthTokens>>
        {
            Success = true,
            Data = result
        };
    }

    public async Task<Result<CreateAuthTokenResponseViewModel>> CreateAuthToken(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return new Result<CreateAuthTokenResponseViewModel>
            {
                Success = false,
                Errors = new List<string> { "Description is required" }
            };
        }
        
        string token;
        
        try
        {
            token = SecureToken.GenerateSecureToken();
        }
        catch (Exception e)
        {
            return new Result<CreateAuthTokenResponseViewModel>
            {
                Success = false,
                Errors = new List<string> { e.Message }
            };
        }
        
        var entry = await applicationDbContext.AuthTokens.AddAsync(new AuthTokens
        {
            Token = token,
            Description = description,
            IsRevoked = false
        });
        
        await applicationDbContext.SaveChangesAsync();

        return new Result<CreateAuthTokenResponseViewModel>
        {
            Success = true,
            Data = new CreateAuthTokenResponseViewModel
            {
                Id = entry.Entity.Id,
                Token = token,
                Description = description,
                CreatedOn = entry.Entity.CreatedOn
            }
        };
    }
    
    public async Task<Result<RevokeTokenResponseViewModel>> RevokeAuthTokenById(int id)
    {
        if (id <= 0)
        {
            return new Result<RevokeTokenResponseViewModel>
            {
                Success = false,
                Errors = new List<string> { "Invalid ID" }
            };
        }
        
        var authToken = await applicationDbContext.AuthTokens
            .SingleOrDefaultAsync(c => c.Id == id && !c.IsRevoked);
        
        if (authToken is null)
        {
            return new Result<RevokeTokenResponseViewModel>
            {
                Success = false,
                Errors = new List<string> { "Token not found" }
            };
        }
        
        authToken.IsRevoked = true;
        authToken.RevokedOn = DateTime.UtcNow;
        
        await applicationDbContext.SaveChangesAsync();
        
        await cacheService.RemoveAsync(CacheKeys.AuthTokens.All);

        return new Result<RevokeTokenResponseViewModel>
        {
            Success = true,
            Data = new RevokeTokenResponseViewModel
            {
                IsRevoked = true
            }
        };
    }
    
    public async Task<Result<RevokeTokenResponseViewModel>> RevokeAuthTokenByToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new Result<RevokeTokenResponseViewModel>
            {
                Success = false,
                Errors = new List<string> { "Token is required" }
            };
        }
        
        var authToken = await applicationDbContext.AuthTokens
            .SingleOrDefaultAsync(c => c.Token == token && !c.IsRevoked);
        
        if (authToken is null)
        {
            return new Result<RevokeTokenResponseViewModel>
            {
                Success = false,
                Errors = new List<string> { "Token not found" }
            };
        }
        
        authToken.IsRevoked = true;
        authToken.RevokedOn = DateTime.UtcNow;
        
        await applicationDbContext.SaveChangesAsync();
        
        await cacheService.RemoveAsync(CacheKeys.AuthTokens.All);

        return new Result<RevokeTokenResponseViewModel>
        {
            Success = true,
            Data = new RevokeTokenResponseViewModel
            {
                Id = authToken.Id,
                IsRevoked = true
            }
        };
    }
    
    public async Task<Result<IsAllowedToken>> IsAllowedToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new Result<IsAllowedToken>
            {
                Success = false,
                Errors = new List<string> { "Token is required" }
            };
        }
        
        var authTokens = await GetAuthTokens();
        
        IsAllowedToken? result;
        
        if (authTokens is { Data: null })
        {
            result = await applicationDbContext.AuthTokens
                .AsNoTracking()
                .ProjectTo<IsAllowedToken>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(c => !c.IsRevoked && c.Id != Generic.System.TokenId && c.Token == token);
        }
        else
        {
            result = mapper.Map<IsAllowedToken>(authTokens.Data
                .SingleOrDefault(c => !c.IsRevoked && c.Id != Generic.System.TokenId && c.Token == token));
        }

        if (result is null)
        {
            return new Result<IsAllowedToken>
            {
                Success = false,
                Errors = new List<string> { "Token not found" }
            };
        }

        return new Result<IsAllowedToken>
        {
            Success = true,
            Data = result
        };
    }
    
    public async Task<Result<List<IPAllowedResponseViewModel>>> GetAllowedIPs()
    {
        var result = await cacheService.GetAsync(CacheKeys.AllowedIPs.All, () =>
        {
            return applicationDbContext.IPsWhitelist
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Id)
                .ProjectTo<IPAllowedResponseViewModel>(mapper.ConfigurationProvider)
                .ToListAsync();
        }, TimeSpan.FromDays(365));

        return new Result<List<IPAllowedResponseViewModel>>
        {
            Success = true,
            Data = result
        };
    }
    
    public async Task<Result<IsAllowedIP>> IsAllowedIP(string ip)
    {
        Regex ipRegex = new(@"^(\d{1,3}\.){3}\d{1,3}$");
        
        if (!ipRegex.IsMatch(ip) || !IPAddress.TryParse(ip, out var ipNetwork2))
        {
            return new Result<IsAllowedIP>
            {
                Success = false,
                Errors = new List<string> { "Invalid IP" }
            };
        }

        var ipsWhitelist = await GetAllowedIPs();
        
        var ipNumeric = IPNetwork2.ToBigInteger(ipNetwork2);

        bool result;

        if (ipsWhitelist is { Data: null })
        {
            result = await applicationDbContext.IPsWhitelist
                .AsNoTracking()
                .AnyAsync(range => 
                    ipNumeric >= range.FirstUsableIPNumeric &&
                    ipNumeric <= range.LastUsableIPNumeric);
        }
        else
        {
            result = ipsWhitelist.Data
                .Any(range => 
                    ipNumeric >= range.FirstUsableIPNumeric &&
                    ipNumeric <= range.LastUsableIPNumeric);
        }
        
        return new Result<IsAllowedIP>
        {
            Success = true,
            Data = new IsAllowedIP
            {
                IP = ip,
                IsAllowed = result
            }
        };
    }
    
    public async Task<Result<AllowIPsResponseViewModel>> AllowIPs(AllowIPsRequestViewModel allowIPsRequestViewModel)
    {
        if (allowIPsRequestViewModel.AllowedIPs == null || allowIPsRequestViewModel.AllowedIPs.Count == 0)
        {
            throw new Exception("IPs are required");
        }
        
        if (string.IsNullOrWhiteSpace(allowIPsRequestViewModel.Description))
        {
            throw new Exception("Description is required");
        }
        
        if (allowIPsRequestViewModel.AllowedIPs.Any(ip => !IPNetwork2.TryParse(ip, sanitanize: false, out _)))
        {
            var invalidIP = allowIPsRequestViewModel.AllowedIPs.First(ip => !IPNetwork2.TryParse(ip, sanitanize: false, out _));
            var invalidIPIndex = allowIPsRequestViewModel.AllowedIPs.IndexOf(invalidIP);
            
            throw new Exception($"Invalid IP [{invalidIP}] on line [{invalidIPIndex+1}]");
        }
        
        var existingIPs = applicationDbContext.IPsWhitelist
            .Where(ip => allowIPsRequestViewModel.AllowedIPs.Contains(ip.IP))
            .ToList();

        if (existingIPs.Any())
        {
            foreach (var ip in existingIPs)
            {
                ip.IsActive = true;
                ip.Description = allowIPsRequestViewModel.Description;
                
                allowIPsRequestViewModel.AllowedIPs.Remove(ip.IP);
            }
        }
        
        var IPsToAdd = allowIPsRequestViewModel.AllowedIPs.Select(ip =>
        {
            IPNetwork2 ipNetwork2 = IPNetwork2.Parse(ip);
            return new IPsWhitelist
            {
                IP = ip,
                Description = allowIPsRequestViewModel.Description,
                FirstUsableIP = ipNetwork2.FirstUsable.ToString(),
                LastUsableIP = ipNetwork2.LastUsable.ToString(),
                FirstUsableIPNumeric = IPNetwork2.ToBigInteger(ipNetwork2.FirstUsable),
                LastUsableIPNumeric = IPNetwork2.ToBigInteger(ipNetwork2.LastUsable),
                IsActive = true
            };
        });

        var ipsToAdd = IPsToAdd.ToList();
        await applicationDbContext.IPsWhitelist.AddRangeAsync(ipsToAdd);

        await applicationDbContext.SaveChangesAsync();
        
        await cacheService.RemoveAsync(CacheKeys.AllowedIPs.All);

        await GetAllowedIPs();
        
        return new Result<AllowIPsResponseViewModel>
        {
            Success = true,
            Data = new AllowIPsResponseViewModel
            {
                IPsImported = ipsToAdd.Count,
                IPsAlreadyAllowed = existingIPs.Count
            }
        };
    }

    // TODO: Test this method, the implementation has not been tested yet.
    public async Task<Result<RevokeIPResponseViewModel>> RevokeIP(string ip)
    {
        Regex ipRegex = new(@"^(\d{1,3}\.){3}\d{1,3}$");
        
        if (!ipRegex.IsMatch(ip) || !IPAddress.TryParse(ip, out var ipNetwork2))
        {
            return new Result<RevokeIPResponseViewModel>
            {
                Success = false,
                Errors = new List<string> { "Invalid IP" }
            };
        }
        
        var ipNumeric = IPNetwork2.ToBigInteger(ipNetwork2);

        var result = applicationDbContext.IPsWhitelist
            .Where(c => c.IP == ip ||
                        (ipNumeric >= c.FirstUsableIPNumeric &&
                         ipNumeric <= c.LastUsableIPNumeric));
        
        await result.ForEachAsync(c => c.IsActive = false);
        
        await applicationDbContext.SaveChangesAsync();

        var rowsAffected = result.Count();
        
        await cacheService.RemoveAsync(CacheKeys.AllowedIPs.All);
        
        return new Result<RevokeIPResponseViewModel>
        {
            Success = true,
            Data = new RevokeIPResponseViewModel
            {
                IP = ip,
                IsRevoked = true,
                RowsAffected = rowsAffected
            }
        };
    }
}