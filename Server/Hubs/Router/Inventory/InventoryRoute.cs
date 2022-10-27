using System.Text.Json;
using Castle.Core.Internal;
using Server.Core.Inventories;
using Server.Database;
using Server.Hubs.Router.Inventory.Requests;
using Server.Web;

namespace Server.Hubs.Router.Inventory;

public class InventoryRoute
{
    private readonly InventoryManager _inventoryManager;

    public InventoryRoute(GameDbContext db, IServiceProvider serviceProvider)
    {
        _inventoryManager = new InventoryManager(db, serviceProvider);
    }

    public async Task<HubResponse> Handle(string command, string request, Guid playerId)
    {
        return command switch
        {
            "getInventory" => await GetInventory(playerId),
            "useGameItemOnSector" => await UseGameItemOnSector(request, playerId),
            _ => new HubResponse()
        };
    }

    private async Task<HubResponse> GetInventory(Guid playerId)
    {
        var inventory = await _inventoryManager.GetByPlayerId(playerId);
        var response = new HubResponse
        {
            Success = true
        };
        if (!inventory!.Items.IsNullOrEmpty())
        {
            response.Data = new
            {
                Items = inventory?.Items.Where(i => i.Quantity >= 0.01).ToList(),
                Sectors = inventory?.Sectors
            };
        }

        return response;
    }

    private async Task<HubResponse> UseGameItemOnSector(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<UseGameItemRequest>(request);
        if (req is null) return new HubResponse() {Success = false};
        var (didUseItem, errorMsg) =
            await _inventoryManager.UseGameItemOnSector(req.SectorName, req.GameItemName, playerId);
        var response = new HubResponse()
        {
            Success = didUseItem
        };
        if (!didUseItem)
            response.Message = errorMsg;
        return response;
    }
}