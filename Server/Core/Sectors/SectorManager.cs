using Microsoft.EntityFrameworkCore;
using Server.Database;

namespace Server.Core.Sectors;

public class SectorManager
{
    private readonly GameDbContext _db;

    public SectorManager(GameDbContext db)
    {
        _db = db;
    }

    public async Task<Tuple<Sector, string>> GetByName(string name)
    {
        var sector = await _db.Sectors
            .Include(s => s.Inventory)
            .ThenInclude(inv => inv!.Account)
            .FirstOrDefaultAsync(g => g.Name == name);
        if (sector is null)
            return new Tuple<Sector, string>(new Sector(), "");
        var owner = "";
        if (sector.Inventory is not null)
        {
            owner = sector.Inventory.Account.Username ?? "";
        }

        return new Tuple<Sector, string>(sector, owner);
    }

    public async Task<bool> ChangeCommissionRate(Guid playerId, string sectorName, double commissionRate)
    {
        var inventory = await _db.Inventories
            .Include(i => i.Account)
            .FirstOrDefaultAsync(i => i.Account.PlayerId == playerId);
        var sector = await _db.Sectors.FirstOrDefaultAsync(s => s.Name == sectorName);
        if (inventory is null || sector is null) return false;

        // Not owner
        if (sector.Inventory != inventory ||
            !sector.Inventory.Account.Username!.Equals(inventory.Account.Username)) return false;

        // Minimum enforced rate
        if (commissionRate is < 0 or > 10) return false;

        // Can only change commission once every 48 hours
        if (sector.CommissionChangeTimestamp > DateTime.UtcNow) return false;

        sector.Commission = commissionRate;
        sector.CommissionChangeTimestamp = DateTime.UtcNow.AddHours(48);

        _db.Sectors.Update(sector);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetFaction(Guid playerId, string sectorName, string faction)
    {
        var sector = await _db.Sectors.FirstOrDefaultAsync(s => s.Name == sectorName);
        if (sector is null) return false;
        var inventory = await _db.Inventories
            .FirstOrDefaultAsync(i => i.Account.PlayerId == playerId);

        // Not owner
        if (sector.Inventory != inventory) return false;

        sector.Faction = faction;

        _db.Sectors.Update(sector);
        return await _db.SaveChangesAsync() == 1;
    }
}