using Server.Database;
using Server.Hubs.Router.Engineering;
using Server.Hubs.Router.Inventory;
using Server.Hubs.Router.Market;
using Server.Hubs.Router.Harvester;
using Server.Hubs.Router.Initialize;
using Server.Hubs.Router.Mission;
using Server.Hubs.Router.Sector;
using Server.Hubs.Router.Wallet;
using Server.Web;

namespace Server.Hubs.Router;

public class HubRouter
{
    private readonly GameDbContext _db;
    private readonly IServiceProvider _serviceProvider;

    public HubRouter(GameDbContext db, IServiceProvider serviceProvider)
    {
        _db = db;
        _serviceProvider = serviceProvider;
    }

    public async Task<HubResponse> Route(string route, string command, string request, Guid playerId)
    {
        return route switch
        {
            "initialize" => await new InitializeRoute(_db, _serviceProvider).Handle(command, request, playerId),
            "inventory" => await new InventoryRoute(_db, _serviceProvider).Handle(command, request, playerId),
            "engineering" => await new EngineeringRoute(_db, _serviceProvider).Handle(command, request, playerId),
            "harvester" => await new HarvesterRoute(_db, _serviceProvider).Handle(command, request, playerId),
            "sector" => await new SectorRoute(_db).Handle(command, request, playerId),
            "market" => await new MarketRoute(_db, _serviceProvider).Handle(command, request, playerId),
            "wallet" => await new WalletRoute(_db).Handle(command, request, playerId),
            "mission" => await new MissionRoute(_db, _serviceProvider).Handle(command, request, playerId),
            _ => new HubResponse() {Success = false}
        };
    }
}