using System.Security.Cryptography;

namespace Application.Common.Utils;

public static class SecureToken
{
    public static string GenerateSecureToken(int length = 32)
    {
        if (length < 32)
        {
            throw new ArgumentException("Token length must be at least 32 characters");
        }
        
        var randomBytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}