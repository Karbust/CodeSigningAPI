using System.Text.Json.Serialization;
using Application.Common.Mappings;

namespace Application.Services.Settings.Models.AuthTokens;

public class IsAllowedToken : IMapFrom<Domain.Entities.AuthTokens>
{
    public int Id { get; set; }
    public string Description { get; set; }

    [JsonIgnore]
    public bool IsRevoked { get; set; }
    
    [JsonIgnore]
    public string Token { get; set; }
    
    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap<Domain.Entities.AuthTokens, IsAllowedToken>();
    }
}