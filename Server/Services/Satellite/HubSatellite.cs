using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Server.Web;

namespace Server.Services.Satellite;

public class HubSatellite : IHubSatellite
{
    private readonly ILogger<HubSatellite> _logger;
    private readonly IHubContext<GameHub> _hubContext;

    public HubSatellite(ILogger<HubSatellite> logger, IHubContext<GameHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task SendToClient(string playerId, string command, string message)
    {
        await _hubContext.Clients.Group(playerId).SendAsync(command, 
            new HubResponse()
            {
                Success = true,
                Message = message
            });
    }
}