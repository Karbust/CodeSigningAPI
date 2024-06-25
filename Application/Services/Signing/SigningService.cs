using System.Diagnostics;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Utils;
using Application.Options;
using Application.Services.Signing.Models;
using Domain.Constants;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services.Signing;

public class SigningService(
    IOptions<SettingOptions> settingOptions,
    IOptions<SigningOptions> signingOptions,
    ILogger<SigningService> logger,
    IApplicationDbContext applicationDbContext,
    ICurrentSessionService currentSessionService
) : ISigningService
{
    public async Task<Result<SignFileResponseViewModel, SigningErrors>> SignFile(SignFileServiceRequestViewModel signFileRequestViewModel)
    {
        var returnFile = Array.Empty<byte>();
        var returnResponse = new List<SigningErrors>();

        if (settingOptions.Value.EnableAuthentication && !currentSessionService.IsAuthenticated)
        {
            returnResponse.Add(new SigningErrors
            {
                Message = "Unauthorized",
                Error = "Unauthorized"
            });
            
            return new Result<SignFileResponseViewModel, SigningErrors>
            {
                Success = false,
                Errors = returnResponse
            };
        }

        var tokenId = currentSessionService.TokenId ?? Generic.System.TokenId;

        AuthTokens? authToken = await applicationDbContext.AuthTokens.FindAsync(tokenId);

        if (signFileRequestViewModel.Algorithms == null || signFileRequestViewModel.Algorithms.Count == 0)
        {
            signFileRequestViewModel.Algorithms = signingOptions.Value.Algorithms.ToList();
        }
        
        var originalHash = FileHash.SHA256CheckSum(signFileRequestViewModel.FilePath);

        foreach (var algorithm in signFileRequestViewModel.Algorithms)
        {
            var arguments = $"-Djava.security.debug=sunpkcs11 -jar {signingOptions.Value.BasePath}\\{signingOptions.Value.BinaryName} " +
                            $"--storetype {signingOptions.Value.KeyStoreType} " +
                            $"--storepass {signingOptions.Value.KeyStorePassword} " +
                            $"--alg {algorithm} " +
                            $"--certfile {Path.Combine(signingOptions.Value.BasePath, signingOptions.Value.CertificateFile)} " +
                            $"--tsaurl {signingOptions.Value.TimeStampUrl} " +
                            $"\"{signFileRequestViewModel.TempFileName}\"";
            
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false,
                RedirectStandardError = true,
                WorkingDirectory = SigningOptions.tempFolder
            };
            
            Process process = new Process { StartInfo = processStartInfo };
            var processStart = process.Start();

            if (processStart)
            {
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                
                if (String.IsNullOrWhiteSpace(error))
                { 
                    logger.LogInformation(output);
                    var hash = FileHash.SHA256CheckSum(signFileRequestViewModel.FilePath);
                    
                    var signedFile = await File.ReadAllBytesAsync(signFileRequestViewModel.FilePath);
                    returnFile = signedFile;

                    var codeSigning = new CodeSigning
                    {
                        FileName = signFileRequestViewModel.FileName,
                        OriginalHash = originalHash,
                        SignedHash = hash,
                        Algorithm = algorithm,
                        SignedBy = authToken,
                        CreatedOn = DateTime.UtcNow
                    };
                    
                    await applicationDbContext.CodeSignings.AddAsync(codeSigning);
                    
                    await applicationDbContext.SaveChangesAsync();
                }
                else
                {
                    logger.LogError(new Exception(error), "Error signing file [{TempFileName}] with algorithm {Algorithm}", signFileRequestViewModel.TempFileName, algorithm);
                    
                    returnResponse.Add(new SigningErrors
                    {
                        Message = error,
                        Error = $"Error signing file with algorithm {algorithm}"
                    });
                }
            }
            
        }
        
        if (returnResponse.Count > 0 || returnFile.Length == 0)
        {
            return new Result<SignFileResponseViewModel, SigningErrors>
            {
                Success = false,
                Errors = returnResponse
            };
        }
        
        return new Result<SignFileResponseViewModel, SigningErrors>
        {
            Success = true,
            Data = new SignFileResponseViewModel { returnFile = returnFile }
        };
    }
}
