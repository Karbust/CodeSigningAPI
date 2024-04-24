using System.ComponentModel.DataAnnotations;

namespace Application.Services.Settings.Models.AuthTokens;

public class RevokeTokenByIdRequestViewModel
{
    [Required]
    public int Id { get; set; }
}