using GameLib.Data.Factions;
using Microsoft.EntityFrameworkCore;
using Server.Database;

namespace Server.Core.Factions;

public class FactionManager
{
    private readonly GameDbContext _db;

    public FactionManager(GameDbContext db)
    {
        _db = db;
    }

    public async Task<bool> AssignFaction(Guid playerId, string factionName)
    {
        if (!GameFaction.IsValidFaction(factionName))
            return false;

        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (account is null) return false;
        var currentFaction = await _db.Factions.FirstOrDefaultAsync(f => f.Account.PlayerId == playerId);
        if (currentFaction is null)
        {
            var faction = new Faction()
            {
                Account = account,
                FactionName = factionName
            };
            await _db.Factions.AddAsync(faction);
            return await _db.SaveChangesAsync() == 1;
        }

        currentFaction.FactionName = factionName;
        _db.Factions.Update(currentFaction);
        return await _db.SaveChangesAsync() == 1;
    }

    public async Task<string> GetPlayerFaction(Guid playerId)
    {
        var faction = await _db.Factions.FirstOrDefaultAsync(f => f.Account.PlayerId == playerId);
        return faction is null ? "Invalid faction." : faction.FactionName;
    }
}