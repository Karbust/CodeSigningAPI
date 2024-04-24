using Application.Common.Models;
using Application.Options;
using Application.Services.Settings.Models.IPs;
using Application.Services.Signing;
using Application.Services.Signing.Models;
using CodeSigningApi.Extensions;
using CodeSigningApi.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CodeSigningApi.Controllers;

[MiddlewareFilter(typeof(TokenAuthenticationMiddlewareExtensions))]
[ApiController]
[Route("api/[controller]")]
public class SignController(
    ILogger<SigningOptions> logger,
    ISigningService signingService
) : ControllerBase
{
    /// <summary>
    /// Signs a file with the provided algorithms. If no algorithms are provided, the default algorithms configured in the appsettings will be used.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="file"></param>
    /// <returns>The signed file</returns>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(FileContentResult), "application/octet-stream")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(Result<SignFileResponseViewModel, SigningErrors>), "application/json")]
    public async Task<ActionResult<Result<SignFileResponseViewModel, SigningErrors>>> SignFile(
        [ModelBinder(BinderType = typeof(JsonModelBinder))] SignFileRequestViewModel? request,
        IFormFile file
    )
    {
        var fileGuid = Guid.NewGuid();
        var tempFileName = $"{fileGuid.ToString()}\\{file.FileName}";
        
        var filePath = $"{SigningOptions.tempFolder}\\{tempFileName}";
        
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
        
        await using var stream = file.OpenReadStream();
        await using var fileStream = System.IO.File.Create(filePath);
        
        await stream.CopyToAsync(fileStream);
        stream.Close();
        fileStream.Close();

        var signFile = await signingService.SignFile(new SignFileServiceRequestViewModel
        {
            FilePath = filePath,
            FileName = file.FileName,
            TempFileName = tempFileName,
            Algorithms = request.Algorithms
        });
        
        System.IO.File.Delete(filePath);
        Directory.Delete(Path.GetDirectoryName(filePath) ?? string.Empty);
        
        if (!signFile.Success || signFile.Data == null)
        {
            return BadRequest(signFile);
        }

        return File(signFile.Data.returnFile, "application/octet-stream", file.FileName);
    }
}