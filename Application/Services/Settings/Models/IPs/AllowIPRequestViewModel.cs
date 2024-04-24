using System.ComponentModel.DataAnnotations;

namespace Application.Services.Settings.Models.IPs;

public class AllowIPRequestViewModel
{
    [Required]
    public string AllowedIP { get; set; }
    [Required]
    public string Description { get; set; }
}