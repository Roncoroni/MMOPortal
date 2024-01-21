using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace MMO.Authentication;

internal sealed class GameServerTokenConfigurationOptions
    (IDataProtectionProvider dp) : IConfigureNamedOptions<GameServerTokenOptions>
{
    internal const string _primaryPurpose = "MMOPortal.Authentication.GameServer";
    
    public void Configure(string? schemeName, GameServerTokenOptions options)
    {
        if (schemeName is null)
        {
            return;
        }

        options.ServerTokenProtector = new TicketDataFormat(dp.CreateProtector(_primaryPurpose, schemeName, "Token"));
    }

    public void Configure(GameServerTokenOptions options)
    {
        throw new NotImplementedException();
    }
}