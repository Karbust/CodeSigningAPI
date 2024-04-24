using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Services.Settings.Models.IPs;

public class AllowedIPsResponseViewModel : IMapFrom<IPsWhitelist>
{
    public string IP { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<IPsWhitelist, AllowedIPsResponseViewModel>();
    }
}