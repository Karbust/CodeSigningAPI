using Application.Common.Exceptions;

namespace Application.Options;

public class SettingOptions
{
    private readonly bool _allowClassAReservedIPs;
    private readonly bool _allowClassBReservedIPs;
    private readonly bool _allowClassCReservedIPs;
    
    public const string Configuration = "Settings";
    
    public bool EnableIPsWhitelist { get; set; }
    
    public bool AllowAllPrivateIanaReservedIPs { get; set; }

    public bool AllowClassAReservedIPs
    {
        get => _allowClassAReservedIPs;
        init
        {
            if (AllowAllPrivateIanaReservedIPs && !value)
            {
                throw new InvalidConfiguration("Cannot disallow Class A reserved IPs when IANA reserved IPs are allowed");
            }
            
            _allowClassAReservedIPs = value;
        }
    }

    public bool AllowClassBReservedIPs
    {
        get => _allowClassBReservedIPs;
        init
        {
            if (AllowAllPrivateIanaReservedIPs && !value)
            {
                throw new InvalidConfiguration("Cannot disallow Class B reserved IPs when IANA reserved IPs are allowed");
            }
            
            _allowClassBReservedIPs = value;
        }
    }

    public bool AllowClassCReservedIPs
    {
        get => _allowClassCReservedIPs;
        init
        {
            if (AllowAllPrivateIanaReservedIPs && !value)
            {
                throw new InvalidConfiguration("Cannot disallow Class C reserved IPs when IANA reserved IPs are allowed");
            }
            
            _allowClassCReservedIPs = value;
        }
    }

    public bool AllowCGNatIPs { get; set; }

    public bool EnableAuthentication { get; set; } = true;
    public bool BypassAuthenticationLoopback { get; set; } = true;
}