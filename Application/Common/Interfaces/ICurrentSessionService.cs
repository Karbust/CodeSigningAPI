using System.Net;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface ICurrentSessionService
{
    bool IsAuthenticated { get; }

    int? TokenId { get; }

    string? TokenDescription { get; }
        
    IPAddress? IpAddress { get; }
}