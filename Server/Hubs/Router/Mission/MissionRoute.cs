using System.Text.Json;
using Server.Core.Mission;
using Server.Database;
using Server.Hubs.Router.Mission.Requests;
using Server.Web;

namespace Server.Hubs.Router.Mission;

public class MissionRoute
{
    private readonly MissionManager _missionManager;

    public MissionRoute(GameDbContext db, IServiceProvider serviceProvider)
    {
        _missionManager = new MissionManager(db, serviceProvider);
    }

    public async Task<HubResponse> Handle(string command, string request, Guid playerId)
    {
        return command switch
        {
            "getMissions" => await GetMissions(),
            "completeMission" => await CompleteMission(request, playerId),
            _ => new HubResponse()
        };
    }
    
    private async Task<HubResponse> GetMissions()
    {
        var missions = await _missionManager.GetActiveMissions();
        var response = new HubResponse()
        {
            Success = missions.Any(),
            Data = new
            {
                Missions = missions
            }
        };
        return response;
    }    
    private async Task<HubResponse> CompleteMission(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<CompleteResourceMissionRequest>(request);
        if (req is null) return new HubResponse() {Success = false};
        
        var reward = await _missionManager.CompleteMission(playerId, req.MissionCode);
        var response = new HubResponse()
        {
            Success = reward > 0,
            Message = $"Earned {reward} EUA."
        };
        if (reward <= 0)
            response.Message = "Mission has already been completed.";
        return response;
    }
}