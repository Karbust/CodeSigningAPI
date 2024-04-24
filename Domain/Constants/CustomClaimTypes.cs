using System.Security.Claims;

namespace Domain.Constants
{
    public static class CustomClaimTypes
    {
        private const string Prefix = "CodeSigningAPI";
    
        public const string TokenId = $"{Prefix}_TokenId";
        public const string Token = $"{Prefix}_Token";
        public const string TokenDescription =$"{Prefix}_TokenDescription";
    }
}