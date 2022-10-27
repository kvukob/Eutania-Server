using Microsoft.AspNetCore.SignalR;

namespace Server.Web;

public static class HubExtensions
{
    public static Guid GetPlayerId(this Hub hub, HubCallerContext context)
    {
        return Guid.TryParse(context.User?.Identity?.Name, out var guid) ? guid : Guid.Empty;
    }
}