using System.Numerics;
using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Services.Settings.Models.IPs;

public class IPAllowedResponseViewModel : IMapFrom<IPsWhitelist>
{
    public int Id { get; set; }
    public string IP { get; set; }
    public string Description { get; set; }
    public string FirstUsableIP { get; set; }
    public string LastUsableIP { get; set; }
    
    /// <summary>
    /// The first usable IP address in numeric format. BigInteger is used to handle large numbers.
    /// </summary>
    public BigInteger FirstUsableIPNumeric { get; set; }
    
    /// <summary>
    /// The last usable IP address in numeric format. BigInteger is used to handle large numbers.
    /// </summary>
    public BigInteger LastUsableIPNumeric { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<IPsWhitelist, IPAllowedResponseViewModel>();
    }
}