using System.ComponentModel.DataAnnotations;

namespace Application.Services.Settings.Models.AuthTokens;

public class RevokeTokenByTokenRequestViewModel
{
    [Required]
    public string Token { get; set; }
}