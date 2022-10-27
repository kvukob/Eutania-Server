using System.Text.Json;
using Server.Core.Harvesters;
using Server.Core.Sectors;
using Server.Database;
using Server.Hubs.Router.Harvester.Requests;
using Server.Web;

namespace Server.Hubs.Router.Harvester;

public class HarvesterRoute
{
    private readonly HarvesterManager _harvesterManager;
    private readonly SectorManager _sectorManager;

    public HarvesterRoute(GameDbContext db, IServiceProvider serviceProvider)
    {
        _harvesterManager = new HarvesterManager(db, serviceProvider);
        _sectorManager = new SectorManager(db);
    }

    public async Task<HubResponse> Handle(string command, string request, Guid playerId)
    {
        return command switch
        {
            "getHarvester" => await GetHarvester(playerId),
            "setHarvesterSector" => await SetHarvesterSector(request, playerId),
            "mine" => await Mine(playerId),
            "equipItem" => await EquipItem(request,playerId),
            _ => new HubResponse()
        };
    }
    
    private async Task<HubResponse> GetHarvester(Guid playerId)
    {
        var harvester = await _harvesterManager.GetByPlayerId(playerId);
        var (sector, owner) = await _sectorManager.GetByName(harvester.Sector.Name);
        var response = new HubResponse
        {
            Success = true,
            Data = new
            {
                OnCooldown = harvester.OnCooldown,
                Cooldown = harvester.Cooldown,
                Sector = new
                {
                    Name = sector.Name,
                    Planet = sector.Planet,
                    Commission = sector.Commission,
                    Rarity = sector.Rarity,
                    Owner = owner
                },
                Tool = harvester.Tool,
                Weapon = harvester.Weapon,
                Protection = harvester.Protection
            }
        };
        return response;
    }
    
    private async Task<HubResponse> SetHarvesterSector(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<SetHarvesterSectorRequest>(request);
        if (req is null) return new HubResponse() {Success = false};
        
        var sector = await _harvesterManager.SetHarvesterSector(playerId, req.SectorName);
        var response = new HubResponse();
        if (sector is not null)
        {
            response.Success = true;
            response.Message = $"Sector set to {req.SectorName}.";
            response.Data = new
            {
                SectorName = sector
            };
        }
        else
        {
            response.Success = false;
            response.Message = "Error setting sector.";
        }

        return response;

    }
    private async Task<HubResponse> Mine(Guid playerId)
    {
        var isMining = await _harvesterManager.IsOnCooldown(playerId);
        if (isMining)
        {
            var response = new HubResponse()
            {
                Success = false,
                Message = "Already mining."
            };
            return response;
        }
        else
        {
            var rewards = await _harvesterManager.Mine(playerId);
            var response = new HubResponse()
            {
                Success = rewards?.Rewards.Count > 0,
                Message = "Mining complete!"
            };
            if (rewards?.Rewards.Count > 0)
            {
                var harvester = await _harvesterManager.GetByPlayerId(playerId);
                response.Data = new
                {
                    OnCooldown = harvester.OnCooldown,
                    Cooldown = harvester.Cooldown,
                    Rewards = rewards.Rewards,
                    MinedSector = rewards.MinedSector,
                    MinedSectorName = rewards.MinedSectorName,
                    MinedSectorPlanet = rewards.MinedSectorPlanet,
                    MinedSectorRarity = rewards.MinedSectorRarity,
                    ItemDropped = rewards.ItemDropped,
                    ItemDropName = rewards.ItemDropName,
                    ItemDropType = rewards.ItemDropType,
                    ItemDropQuantity = rewards.ItemDropQuantity,
                    ItemDropMintNumber = rewards.ItemDropMintNumber,
                    ItemDropFoil = rewards.ItemDropFoil
                };
            }
            return response;
        }
    }

    private async Task<HubResponse> EquipItem(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<EquipHarvesterItemRequest>(request);
        if (req is null) return new HubResponse() {Success = false};
        
        var didEquip = await _harvesterManager.EquipItem(playerId, req.ItemName);
        var response = new HubResponse()
        {
            Success = didEquip,
            Message = $"Equipped {req.ItemName}."
        };
        return response;
    }
}