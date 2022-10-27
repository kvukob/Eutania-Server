using System.Text.Json;
using Server.Core.Engineering;
using Server.Database;
using Server.Hubs.Router.Engineering.Requests;
using Server.Web;

namespace Server.Hubs.Router.Engineering;

public class EngineeringRoute
{
    private readonly EngineeringManager _engineeringManager;
    public EngineeringRoute(GameDbContext db, IServiceProvider serviceProvider)
    {
        _engineeringManager = new EngineeringManager(db, serviceProvider);
    }

    public async Task<HubResponse> Handle(string command, string request, Guid playerId)
    {
        return command switch
        {
            "craftItem" => await CraftItem(request, playerId),
            _ => new HubResponse()
        };
    }
    
    private async Task<HubResponse> CraftItem(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<CraftItemRequest>(request);
        if (req is null) return new HubResponse() {Success = false};

        var (isCrafted, msg) = await _engineeringManager.CraftItem(playerId, req.GameItemName);
        
        var response = new HubResponse()
        {
            Success = isCrafted,
            Message = msg,
        };
        return response;
    }
}