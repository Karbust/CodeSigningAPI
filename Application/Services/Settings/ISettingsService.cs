using Application.Common.Models;
using Application.Services.Settings.Models.AuthTokens;
using Application.Services.Settings.Models.IPs;
using Domain.Entities;

namespace Application.Services.Settings;

public interface ISettingsService
{
    Task<Result<List<AuthTokens>>> GetAuthTokens();
    Task<Result<CreateAuthTokenResponseViewModel>> CreateAuthToken(string description);
    Task<Result<RevokeTokenResponseViewModel>> RevokeAuthTokenById(int id);
    Task<Result<RevokeTokenResponseViewModel>> RevokeAuthTokenByToken(string token);
    Task<Result<IsAllowedToken>> IsAllowedToken(string token);
    Task<Result<List<IPAllowedResponseViewModel>>> GetAllowedIPs();
    Task<Result<IsAllowedIP>> IsAllowedIP(string ip);
    Task<Result<AllowIPsResponseViewModel>> AllowIPs(AllowIPsRequestViewModel allowIPsRequestViewModel);
    Task<Result<RevokeIPResponseViewModel>> RevokeIP(string ip);
}