using Domain.Common;

namespace Domain.Entities;

public class AuthTokens : AuditableEntity
{
    public int Id { get; set; }
    public string Token { get; set; }
    public string Description { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedOn { get; set; }
}