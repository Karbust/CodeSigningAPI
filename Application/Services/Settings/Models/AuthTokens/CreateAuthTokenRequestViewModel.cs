using System.ComponentModel.DataAnnotations;

namespace Application.Services.Settings.Models.AuthTokens;

public class CreateAuthTokenRequestViewModel
{
    [Required]
    public string Description { get; set; }
}