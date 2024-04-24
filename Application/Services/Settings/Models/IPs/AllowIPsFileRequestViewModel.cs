using System.ComponentModel.DataAnnotations;

namespace Application.Services.Settings.Models.IPs;

public class AllowIPsFileRequestViewModel
{
    [Required]
    public string Description { get; set; }
}