namespace Application.Services.Signing.Models;

public class SignFileServiceRequestViewModel
{
    public string TempFileName { get; set; }
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public List<string>? Algorithms { get; set; }
}