namespace Application.Services.Settings.Models.IPs;

public class RevokeIPResponseViewModel
{
    public string IP { get; set; }
    public bool IsRevoked { get; set; }
    public int RowsAffected { get; set; }
}