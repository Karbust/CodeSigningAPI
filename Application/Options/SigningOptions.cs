namespace Application.Options;

public class SigningOptions
{
    public const string Configuration = "Signing";

    public string BasePath { get; set; } = "C:\\jsign";
    public string BinaryName { get; set; } = "jsign.jar";
    public string KeyStoreType { get; set; } = "PIV";
    public string KeyStorePassword { get; set; }
    public string CertificateFile { get; set; } = "user.crt";
    public string TimeStampUrl { get; set; } = "http://timestamp.sectigo.com";

    private string[] AlgorithmsToSign { get; init; } = Array.Empty<string>();

    public string[] Algorithms
    {
        get => AlgorithmsToSign;
        init
        {
            if (value.Length == 0)
            {
                AlgorithmsToSign = ["SHA-1", "SHA-256"];
                return;
            }

            AlgorithmsToSign = value;
        }
    }

    public static readonly string tempFolder = Path.GetTempPath() + "CodeSigningApi";
}