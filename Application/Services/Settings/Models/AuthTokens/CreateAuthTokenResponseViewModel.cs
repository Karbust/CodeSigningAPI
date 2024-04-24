namespace Application.Services.Settings.Models.AuthTokens;

public class CreateAuthTokenResponseViewModel
{
    public int Id { get; set; }
    public string Token { get; set; }
    public string Description { get; set; }
    public DateTime CreatedOn { get; set; }
}