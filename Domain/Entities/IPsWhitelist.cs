using System.Numerics;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public class IPsWhitelist : AuditableEntity
{
    public int Id { get; set; }
    public string IP { get; set; }
    public string Description { get; set; }
    public string FirstUsableIP { get; set; }
    public string LastUsableIP { get; set; }
    public BigInteger FirstUsableIPNumeric { get; set; }
    public BigInteger LastUsableIPNumeric { get; set; }
    public bool IsActive { get; set; } = true;
}