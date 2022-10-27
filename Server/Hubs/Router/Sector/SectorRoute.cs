using System.Text.Json;
using Server.Core.Sectors;
using Server.Database;
using Server.Hubs.Router.Sector.Requests;
using Server.Web;

namespace Server.Hubs.Router.Sector;

public class SectorRoute
{
    private readonly SectorManager _sectorManager;

    public SectorRoute(GameDbContext db)
    {
        _sectorManager = new SectorManager(db);
    }

    public async Task<HubResponse> Handle(string command, string request, Guid playerId)
    {
        return command switch
        {
            "changeCommissionRate" => await ChangeCommissionRate(request, playerId),
            "setFaction" => await SetFaction(request, playerId),
            _ => new HubResponse()
        };
    }

    private async Task<HubResponse> ChangeCommissionRate(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<ChangeCommissionRateRequest>(request);
        if (req is null) return new HubResponse() {Success = false};
        var success = await _sectorManager.ChangeCommissionRate(playerId, req.SectorName, req.Rate);
        var response = new HubResponse()
        {
            Success = success,
            Message = "Commission rate changed."
        };
        return response;
    }
    private async Task<HubResponse> SetFaction(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<SetSectorFactionRequest>(request);
        if (req is null) return new HubResponse() {Success = false};
        var success = await _sectorManager.SetFaction(playerId, req.SectorName, req.FactionName);
        var response = new HubResponse()
        {
            Success = success,
            Message = $"Faction set to {req.FactionName}."
        };
        return response;
    }
}