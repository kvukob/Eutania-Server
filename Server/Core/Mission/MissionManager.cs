using Microsoft.EntityFrameworkCore;
using Server.Core.Inventories;
using Server.Core.Wallets;
using Server.Database;

namespace Server.Core.Mission;

public class MissionManager
{
    private readonly GameDbContext _db;
    private readonly WalletManager _walletManager;
    private readonly InventoryManager _inventoryManager;

    public MissionManager(GameDbContext db, IServiceProvider serviceProvider)
    {
        _db = db;
        _walletManager = new WalletManager(db);
        _inventoryManager = new InventoryManager(db, serviceProvider);
    }

    public async Task<IEnumerable<Mission>> GetActiveMissions()
    {
        return await _db.Missions
            .Include(m => m.Requirements)
                .ThenInclude(r => r.Item)
            .Where(rQ => rQ.Active)
            .ToListAsync();
    }

    public async Task<double> CompleteMission(Guid playerId, string missionCode)
    {
        var inventory = await _inventoryManager.GetByPlayerId(playerId);
        var mission = await _db.Missions
            .Include(m => m.Requirements)
            .FirstOrDefaultAsync(m => m.MissionCode == missionCode);
        if (inventory is null || mission is null) return -1;
        if (!mission.Active) return -1;
        
        var hasRequirements = CheckRequirements(mission.Requirements, inventory);
        if (hasRequirements)
        {
            // Remove resources from inventory
            inventory = RemoveRequirements(mission.Requirements, inventory);

            // Add reward to wallet
            var wallet = await _walletManager.GetByPlayerId(playerId);
            wallet.Balance += mission.Reward;

            mission.CompletionCount += 1;

            if (mission.CompletionCount >= mission.MaxCompletions)
            {
                mission.CompletionCount = 0;
                mission.Active = false;
                await ActivateMission();
            }

            _db.Missions.Update(mission);
            _db.Inventories.Update(inventory);
            _db.Wallets.Update(wallet);

            await _db.SaveChangesAsync();
            return mission.Reward;
        }

        return -1;
    }


    private static bool CheckRequirements(IEnumerable<MissionRequirement> requirements, Inventory inventory)
    {
        foreach (var requirement in requirements)
        {
            var owned = inventory.Items.FirstOrDefault(i => i.Item?.Name == requirement.Item.Name);
            if (owned is null) return false;
            if (owned.Quantity < requirement.Quantity) return false;
        }
        return true;
    }

    private static Inventory RemoveRequirements(IEnumerable<MissionRequirement> requirements, Inventory inventory)
    {
        foreach (var requirement in requirements)
        {
            var owned = inventory.Items.First(i => i.Item?.Name == requirement.Item.Name);
            owned.Quantity -= requirement.Quantity;
        }

        return inventory;
    }

    private async Task ActivateMission()
    {
        var missionCount = await _db.Missions.Where(m => m.Active).CountAsync();
        if (missionCount < 4)
        {
            var toActivate = await _db.Missions.FirstAsync(rM => !rM.Active);
            toActivate.Active = true;
            _db.Missions.Update(toActivate);
            await _db.SaveChangesAsync();
            await GetActiveMissions();
        }
    }
}