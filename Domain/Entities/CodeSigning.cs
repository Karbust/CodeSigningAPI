using Domain.Common;

namespace Domain.Entities;

public class CodeSigning : AuditableEntity
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string OriginalHash { get; set; }
    public string SignedHash { get; set; }
    public string Algorithm { get; set; }
    public virtual AuthTokens SignedBy { get; set; }
}