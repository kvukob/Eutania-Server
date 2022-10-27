using System.Text.Json;
using Server.Core.Utility;
using Server.Database;
using Server.Hubs.Router.Initialize.Requests;
using Server.Web;

namespace Server.Hubs.Router.Initialize;

public class InitializeRoute
{
    private readonly GameInitializer _gameInitializer;

    public InitializeRoute(GameDbContext db, IServiceProvider serviceProvider)
    {
        _gameInitializer = new GameInitializer(db, serviceProvider);
    }

    public async Task<HubResponse> Handle(string command, string request, Guid playerId)
    {
        return command switch
        {
            "initialize" => await Initialize(request, playerId),
            _ => new HubResponse()
        };
    }
    
    private async Task<HubResponse> Initialize(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<InitializeRequest>(request);
        if (req is null) return new HubResponse() {Success = false, Message = "Invalid initializer object."};
        
        
        var (initialized, message) = await _gameInitializer.InitializeGame(playerId, req);
        
        var response = new HubResponse()
        {
            Success = initialized,
            Message = message
        };

        return response;
    }
}