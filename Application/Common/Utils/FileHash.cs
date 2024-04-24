using System.Security.Cryptography;
using FileStream = System.IO.FileStream;

namespace Application.Common.Utils;

public static class FileHash
{
    public static string SHA256CheckSum(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var fileStream = File.OpenRead(filePath);
        return BitConverter.ToString(sha256.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
    }
}