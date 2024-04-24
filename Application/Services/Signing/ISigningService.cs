using Application.Common.Models;
using Application.Services.Signing.Models;

namespace Application.Services.Signing;

public interface ISigningService
{
    Task<Result<SignFileResponseViewModel, SigningErrors>> SignFile(SignFileServiceRequestViewModel request);
}