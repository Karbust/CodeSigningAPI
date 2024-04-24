using System.Net;
using Application.Common.Models;
using Application.Services.Settings;
using Application.Services.Settings.Models.AuthTokens;
using Application.Services.Settings.Models.IPs;
using CodeSigningApi.Extensions;
using CodeSigningApi.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CodeSigningApi.Controllers;

[MiddlewareFilter(typeof(TokenAuthenticationMiddlewareExtensions))]
[ApiController]
[Route("api/[controller]")]
public class SettingsController(
    ILogger<SettingsController> logger,
    ISettingsService settingsService
) : ControllerBase
{
    /// <summary>
    /// Checks if the provided IP is allowed in the whitelist.
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    /// <remarks>
    /// This endpoint is used to check if a provided IP address is allowed in the whitelist. \
    /// It receives a string parameter `ip` which represents the IP address to be checked.
    ///
    /// The response will be a JSON object of type <typeparam name="IsAllowedIP">IsAllowedIP</typeparam>.
    /// 
    /// Example:
    /// <code>
    /// {
    ///     "ip": "4.149.10.20",
    ///     "isAllowed": true
    /// }
    /// </code>
    /// </remarks>
    [HttpGet("IsAllowedIP")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(Result<IsAllowedIP>), "application/json")]
    public async Task<ActionResult<Result<IsAllowedIP>>> IsAllowedIP(string ip)
    {
        return await settingsService.IsAllowedIP(ip);
    }
    
    /// <summary>
    /// Returns a list of all the allowed (IsActive) IPs in the whitelist.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// This endpoint retrieves a list of all the allowed (IsActive) IP addresses or IP ranges in the whitelist.
    ///
    /// The response will be a JSON array of objects of type <typeparam name="IPAllowedResponseViewModel">IPAllowedResponseViewModel</typeparam>. \
    /// Each object includes the IP address or IP range, a description, and additional details about the IP range.
    ///
    /// Example:
    /// <code>
    /// [
    ///     {
    ///         "id": 1,
    ///         "ip": "4.149.10.20/32",
    ///         "description": "GitHub Actions",
    ///         "firstUsableIP": "4.149.10.20",
    ///         "lastUsableIP": "4.149.10.20",
    ///         "firstUsableIPNumeric": 76876308,
    ///         "lastUsableIPNumeric": 76876308
    ///     },
    ///     {
    ///         "id": 2,
    ///         "ip": "4.148.0.0/16",
    ///         "description": "GitHub Actions",
    ///         "firstUsableIP": "4.148.0.1",
    ///         "lastUsableIP": "4.148.255.254",
    ///         "firstUsableIPNumeric": 76808193,
    ///         "lastUsableIPNumeric": 76873726
    ///     }
    /// ]
    /// </code>
    ///
    /// Note: Only the IP addresses or ranges that are active (IsActive is true) are returned.
    /// </remarks>
    [HttpGet("AllowedIPs")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request")]
    public async Task<ActionResult<Result<List<IPAllowedResponseViewModel>>>> GetAllowedIPs()
    {
        return await settingsService.GetAllowedIPs();
    }
    
    /// <summary>
    /// Allows to add a single IP or IP range (CIDR) to the whitelist. Works with IPv4 and IPv6.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <remarks>
    /// This endpoint allows the addition of a single IP address or IP range (in CIDR notation) to the whitelist. \
    /// The request body should contain a JSON object of type <typeparam name="AllowIPRequestViewModel">AllowIPRequestViewModel</typeparam>. \
    /// This object includes an IP address or IP range and a description for the IP being added.
    ///
    /// Example:
    /// <code>
    /// {
    ///     "allowedIP": "4.149.10.20/32",
    ///     "description": "GitHub Actions"
    /// }
    /// </code>
    ///
    /// The response will be a JSON object of type <typeparam name="AllowIPsResponseViewModel">AllowIPsResponseViewModel</typeparam>.
    /// This object includes the number of IPs imported and the number of IPs that were already allowed.
    ///
    /// Example:
    /// <code>
    /// {
    ///     "iPsImported": 1,
    ///     "iPsAlreadyAllowed": 0
    /// }
    /// </code>
    ///
    /// Note: If an IP address or range is already in the whitelist, it will not be added again, and it will be counted as an already allowed IP. However, the description will be updated and the IP will be set as active.
    /// </remarks>
    [HttpPost("AllowIP")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(Result<AllowIPsResponseViewModel>), "application/json")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(Result<AllowIPsResponseViewModel>), "application/json")]
    public async Task<ActionResult<Result<AllowIPsResponseViewModel>>> AllowIP([FromBody] AllowIPRequestViewModel request)
    {
        AllowIPsRequestViewModel allowIPsRequestViewModel = new()
        {
            AllowedIPs = [request.AllowedIP],
            Description = request.Description
        };
        
        var result = await settingsService.AllowIPs(allowIPsRequestViewModel);
        return Ok(result);
    }
    
    /// <summary>
    /// Allows to add a list of IPs to the whitelist from an array of IPs or IP ranges (CIDR). Works with IPv4 and IPv6.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <remarks>
    /// This endpoint allows the addition of multiple IP addresses or IP ranges (in CIDR notation) to the whitelist. \
    /// The request body should contain a JSON object of type <typeparam name="AllowIPsRequestViewModel">AllowIPsRequestViewModel</typeparam>. \
    /// This object includes an array of IP addresses or IP ranges and a description for the IPs being added.
    /// 
    /// Example:
    /// <code>
    /// {
    ///     "allowedIPs": ["4.149.10.20", "4.148.0.0/18"],
    ///     "description": "GitHub Actions"
    /// }
    /// </code>
    /// 
    /// The response will be a JSON object of type <typeparam name="AllowIPsResponseViewModel">AllowIPsResponseViewModel</typeparam>. 
    /// This object includes the number of IPs imported and the number of IPs that were already allowed.
    /// 
    /// Example:
    /// <code>
    /// {
    ///     "iPsImported": 2,
    ///     "iPsAlreadyAllowed": 0
    /// }
    /// </code>
    /// 
    /// Note: If an IP address or range is already in the whitelist, it will not be added again, and it will be counted as an already allowed IP. However, the description will be updated and the IP will be set as active.
    /// </remarks>
    [HttpPost("AllowIPs")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(Result<AllowIPsResponseViewModel>), "application/json")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(Result<AllowIPsResponseViewModel>), "application/json")]
    public async Task<ActionResult<Result<AllowIPsResponseViewModel>>> AllowIPs([FromBody] AllowIPsRequestViewModel request)
    {
        var result = await settingsService.AllowIPs(request);
        return Ok(result);
    }
    
    /// <summary>
    /// Allows to add a list of IPs to the whitelist from a text file with one IP or IP range (CIDR) per line. Works with IPv4 and IPv6.
    /// </summary>
    /// <param name="request">This parameter must be passed as a JSON string.</param>
    /// <param name="file"></param>
    /// <returns></returns>
    /// <remarks>
    /// The file should be a text file with one IP or IP range (CIDR) per line. \
    /// The file should be uploaded as a form-data with the key "file". \
    /// The request field should be a JSON object of type <typeparam name="AllowIPsFileRequestViewModel">AllowIPsFileRequestViewModel</typeparam> with the description for the IPs being added.
    ///
    /// Example:
    /// <code>
    /// curl --location 'https://localhost:7165/api/Settings/AllowIPsFile' \
    /// --header 'Authorization: Bearer ' \
    /// --form 'file=@"/C:/ips.txt"' \
    /// --form 'request="{\"description\":\"GitHub Actions\"}"'
    /// </code>
    /// </remarks>
    [HttpPost("AllowIPsFile")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(Result<AllowIPsResponseViewModel>), "application/json")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(Result<AllowIPsResponseViewModel>), "application/json")]
    public async Task<ActionResult<Result<AllowIPsResponseViewModel>>> AllowIPs(
        [ModelBinder(BinderType = typeof(JsonModelBinder))] AllowIPsFileRequestViewModel request,
        IFormFile file
    )
    {
        var ips = new List<string>();
        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        var lineNum = 0;
        while (!reader.EndOfStream)
        {
            lineNum++;
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line)) continue;
            
            if (!IPNetwork2.TryParse(line, sanitanize: false, out _))
            {
                return BadRequest($"Invalid IP [{line}] on line [{lineNum}]");
            }
                
            ips.Add(line);
        }
        
        AllowIPsRequestViewModel allowIPsRequestViewModel = new()
        {
            AllowedIPs = ips,
            Description = request.Description
        };
        
        var result = await settingsService.AllowIPs(allowIPsRequestViewModel);
        return Ok(result);
    }
    
    /// <summary>
    /// Revokes an IP from the whitelist by its value.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <remarks>
    /// **TODO:** The implementation has not yet been tested.
    /// 
    /// This endpoint allows the revocation of an IP address or IP range by its value. \
    /// The request body should contain a JSON object of type <typeparam name="RevokeIPRequestViewModel">RevokeIPRequestViewModel</typeparam>.
    ///
    /// Example:
    /// <code>
    /// {
    ///     "ip": "4.149.10.20",
    /// }
    /// </code>
    /// 
    /// OR
    /// 
    /// <code>
    /// {
    ///     "ip": "4.148.0.0/18",
    /// }
    /// </code>
    ///
    ///
    /// The response will be a JSON object of type <typeparam name="RevokeIPResponseViewModel">RevokeIPResponseViewModel</typeparam>..
    ///
    /// Example:
    /// <code>
    /// {
    ///     "ip": "4.149.10.20",
    ///     "isRevoked": true
    /// }
    /// </code>
    ///
    /// Note: If the IP address or range does not exist or is already revoked, the endpoint will return an error. \
    /// Note 2: If passed a single IP address and it is part of a range (or ranges), the entire range(s) will be revoked. If there is an entry for the IP address and the range, both will be revoked.
    /// </remarks>
    [HttpPut("RevokeIP")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(Result<RevokeIPResponseViewModel>), "application/json")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(Result<RevokeIPResponseViewModel>), "application/json")]
    public async Task<ActionResult<Result<RevokeIPResponseViewModel>>> RevokeIP([FromBody] RevokeIPRequestViewModel request)
    {
        return await settingsService.RevokeIP(request.IP);
    }
    
    /// <summary>
    /// Creates a new authentication token.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <remarks>
    /// This endpoint allows the creation of a new authentication token. \
    /// The request body should contain a JSON object of type <typeparam name="CreateAuthTokenRequestViewModel">CreateAuthTokenRequestViewModel</typeparam>. \
    /// This object includes a description for the token being created.
    ///
    /// Example:
    /// <code>
    /// {
    ///     "description": "Test"
    /// }
    /// </code>
    ///
    /// The response will be a JSON object of type <typeparam name="CreateAuthTokenResponseViewModel">CreateAuthTokenResponseViewModel</typeparam>. \
    /// This object includes the ID of the created token, the token value, the description, and the date the token was created.
    ///
    /// Example:
    /// <code>
    /// {
    ///     "id": 6,
    ///     "token": "MvEK+zpZ3guY01Py8fTbxstFaQo+OMyNmA93ayMcPuU=",
    ///     "description": "Test",
    ///     "createdOn": "2024-02-23T12:17:48.0006045Z"
    /// }
    /// </code>
    /// 
    /// </remarks>
    [HttpPost("CreateAuthToken")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(Result<CreateAuthTokenResponseViewModel>), "application/json")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(Result<CreateAuthTokenResponseViewModel>), "application/json")]
    public async Task<ActionResult<Result<CreateAuthTokenResponseViewModel>>> CreateAuthToken([FromBody] CreateAuthTokenRequestViewModel request)
    {
        return await settingsService.CreateAuthToken(request.Description);
    }
    
    /// <summary>
    /// Revokes an authentication token by its ID.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <remarks>
    /// This endpoint allows the revocation of an authentication token by its ID. \
    /// The request body should contain a JSON object of type <typeparam name="RevokeTokenByIdRequestViewModel">RevokeTokenByIdRequestViewModel</typeparam>. \
    /// This object includes the token value that needs to be revoked.
    ///
    /// Example:
    /// <code>
    /// {
    ///     "id": 1
    /// }
    /// </code>
    ///
    /// The response will be a JSON object of type <typeparam name="RevokeTokenResponseViewModel">RevokeTokenResponseViewModel</typeparam>. \
    /// This object includes the ID of the revoked token and a boolean indicating whether the revocation was successful.
    ///
    /// Example:
    /// <code>
    /// {
    ///     "id": 1,
    ///     "isRevoked": true
    /// }
    /// </code>
    ///
    /// Note: If the token value does not exist or is already revoked, the endpoint will return an error.
    /// </remarks>
    [HttpPut("RevokeAuthTokenById")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(Result<RevokeTokenResponseViewModel>), "application/json")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(Result<RevokeTokenResponseViewModel>), "application/json")]
    public async Task<ActionResult<Result<RevokeTokenResponseViewModel>>> RevokeAuthTokenById([FromBody] RevokeTokenByIdRequestViewModel request)
    {
        return await settingsService.RevokeAuthTokenById(request.Id);
    }
    
    /// <summary>
    /// Revokes an authentication token by its value.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <remarks>
    /// This endpoint allows the revocation of an authentication token by its value. \
    /// The request body should contain a JSON object of type <typeparam name="RevokeTokenByTokenRequestViewModel">RevokeTokenByTokenRequestViewModel</typeparam>. \
    /// This object includes the token value that needs to be revoked.
    ///
    /// Example:
    /// <code>
    /// {
    ///     "token": "example_token_value"
    /// }
    /// </code>
    ///
    /// The response will be a JSON object of type <typeparam name="RevokeTokenResponseViewModel">RevokeTokenResponseViewModel</typeparam>. \
    /// This object includes the ID of the revoked token and a boolean indicating whether the revocation was successful.
    ///
    /// Example:
    /// <code>
    /// {
    ///     "id": 1,
    ///     "isRevoked": true
    /// }
    /// </code>
    ///
    /// Note: If the token value does not exist or is already revoked, the endpoint will return an error.
    /// </remarks>
    [HttpPut("RevokeAuthTokenByToken")]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(Result<RevokeTokenResponseViewModel>), "application/json")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(Result<RevokeTokenResponseViewModel>), "application/json")]
    public async Task<ActionResult<Result<RevokeTokenResponseViewModel>>> RevokeAuthTokenByToken([FromBody] RevokeTokenByTokenRequestViewModel request)
    {
        return await settingsService.RevokeAuthTokenByToken(request.Token);
    }
}