using System.ComponentModel.DataAnnotations;

namespace Application.Services.Settings.Models.IPs;

public class AllowIPsRequestViewModel
{
    [Required]
    public List<string> AllowedIPs { get; set; }
    [Required]
    public string Description { get; set; }
}