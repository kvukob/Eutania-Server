using Microsoft.AspNetCore.Mvc;

namespace Server.Web;

public static class ControllerBaseExtensions
{
    public static Guid GetPlayerId(this ControllerBase controller, HttpContext context)
    {
        return Guid.TryParse(context.User.Identity?.Name, out var guid) ? guid : Guid.Empty;
    }
}