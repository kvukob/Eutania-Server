using GameLib.Data.Planets;
using Microsoft.EntityFrameworkCore;
using Server.Core._misc;
using Server.Core.Accounts;
using Server.Core.Accounts.Entities;
using Server.Core.Harvesters.DropGenerator;
using Server.Core.Harvesters.Entities;
using Server.Core.Harvesters.Models;
using Server.Core.Inventories;
using Server.Database;
using Server.Services.Effex;

namespace Server.Core.Harvesters;

public class HarvesterManager
{
    private readonly GameDbContext _db;
    private readonly InventoryManager _inventoryManager;
    private readonly IServiceProvider _serviceProvider;

    public HarvesterManager(GameDbContext db, IServiceProvider serviceProvider)
    {
        _db = db;
        _inventoryManager = new InventoryManager(db, serviceProvider);
        _serviceProvider = serviceProvider;
    }

    public async Task<Harvester> GetByPlayerId(Guid playerId)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (account is null) return new Harvester();
        var harvester = await _db.Harvesters
            .Include(h => h.Sector)
            .FirstOrDefaultAsync(m => m.Account == account);
        if (harvester!.Cooldown < DateTime.UtcNow)
        {
            harvester.OnCooldown = false;
            _db.Harvesters.Update(harvester);
            await _db.SaveChangesAsync();
        }

        return harvester!;
    }

    public async Task<string?> SetHarvesterSector(Guid playerId, string sectorName)
    {
        var harvester = await _db.Harvesters.FirstOrDefaultAsync(m => m.Account.PlayerId == playerId);
        var sector = await _db.Sectors.FirstOrDefaultAsync(s => s.Name == sectorName);
        if (harvester is null || sector is null) return null;
        harvester.Sector = sector;
        _db.Update(harvester);
        var success = await _db.SaveChangesAsync();
        return success == 1 ? sectorName : null;
    }

    public async Task<bool> IsOnCooldown(Guid playerId)
    {
        var harvester = await _db.Harvesters.FirstOrDefaultAsync(m => m.Account.PlayerId == playerId);
        if (harvester is null) return true;

        // Returns true if harvester is still on cooldown
        if (harvester.Cooldown >= DateTime.UtcNow) return harvester.OnCooldown;

        // Updates harvester if it's off cooldown
        harvester.OnCooldown = false;
        _db.Harvesters.Update(harvester);
        await _db.SaveChangesAsync();
        return false;
    }

    public async Task<MiningRewardDto?> Mine(Guid playerId)
    {
        var harvester = await _db.Harvesters
            .Include(h => h.Sector)
            .Include(h => h.Tool)
            .ThenInclude(t => t!.Item)
            .Include(h => h.Weapon)
            .ThenInclude(w => w!.Item)
            .Include(p => p.Protection)
            .ThenInclude(t => t!.Item)
            .FirstOrDefaultAsync(m => m.Account.PlayerId == playerId);
        if (harvester is null) return null;

        var playerInventory = await _inventoryManager.GetByPlayerId(playerId);
        var playerFaction = await _db.Factions.FirstOrDefaultAsync(f => f.Account.PlayerId == playerId);
        if (playerInventory is null || playerFaction is null) return null;

        var miningRewardDto = new MiningRewardDto();
        var rand = new Random();

        var effex = _serviceProvider.GetRequiredService<IEffex>();
        var sectorEffects = effex.GetEffectsBySector(harvester.Sector.Name);

        var (miningConfig, configuredHarvester) = 
            new MiningConfiguration().Configure(effex, harvester, sectorEffects);
        harvester = configuredHarvester;

        // Game items for the resources being mined
        var resourcesToMine = await GetResourceGameItems(harvester.Sector.Planet);

        var sector = await _db.Sectors
            .Include(s => s.Inventory)
            .ThenInclude(i => i.Items)
            .ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(s => s.Name == harvester.Sector.Name);
        if (sector is null) return null;
        var isSectorOwned = sector.Inventory is not null;
        var isSectorForSale = sector.ForSale;
        var isOwnedBySelf = sector.Inventory == playerInventory;
        var isPayingCommission = (isSectorOwned && !isSectorForSale && !isOwnedBySelf);

        // Sector Effects, Commission Rate, Harvester Tool Bonus, Yield Loss, Raid Loss
        foreach (var resource in resourcesToMine)
        {
            var inventoryItem = playerInventory.Items.FirstOrDefault(i => i.Item.Id == resource.Id);
            var resourceYield = PlanetRepository.GetMiningYield(harvester.Sector.Planet, resource.Name);

            var resourceDto = new ResourceReward();

            // SECTOR EFFECTS
            //
            //
            var miningYieldEffect = sectorEffects.FirstOrDefault(e => e.Type == "MiningYield");
            if (miningYieldEffect is not null)
            {
                resourceYield *= miningYieldEffect.Modifier;
                effex.UseEffect(sector.Name, miningYieldEffect);
            }

            var raidChanceEffect = sectorEffects.FirstOrDefault(e => e.Type == "RaidChance");
            if (raidChanceEffect is not null)
            {
                miningConfig.RaidChance *= raidChanceEffect.Modifier;
                effex.UseEffect(sector.Name, raidChanceEffect);
            }


            // COMMISSION
            //
            //
            if (isPayingCommission && sector.Inventory is not null)
            {
                var commissionAmount = Math.Round(resourceYield * (harvester.Sector.Commission / 100), 2);
                // Take commission from players reward
                resourceYield -= commissionAmount;
                // Grant commission to sector owner
                var exists = sector.Inventory.Items.FirstOrDefault(i => i.Item.Name == resource.Name);
                if (exists is null)
                {
                    var newItem = new InventoryItem()
                    {
                        Inventory = sector.Inventory,
                        Item = await _db.GameItems.FirstAsync(i => i.Name == resource.Name),
                        Quantity = commissionAmount
                    };
                    await _db.InventoryItems.AddAsync(newItem);
                }
                else
                {
                    exists.Quantity += commissionAmount;
                    _db.InventoryItems.Update(exists);
                }

                resourceDto.Commission = commissionAmount;
            }

            // HARVESTER TOOL BONUS
            //
            //
            if (resource.Name == miningConfig.BonusResourceName)
            {
                var bonusAmount = Math.Round((resourceYield * miningConfig.BonusResourceFactor) - resourceYield, 2);
                resourceYield += bonusAmount;
                resourceDto.Bonus = true;
                resourceDto.BonusQuantity = bonusAmount;
            }

            // YIELD LOSS
            //
            //
            var yieldLossPercent =
                rand.NextDouble() *
                (miningConfig.YieldLossMaximum - miningConfig.YieldLossMinimum) + miningConfig.YieldLossMinimum;
            var yieldLoss = Math.Round(resourceYield * yieldLossPercent, 2);
            resourceYield -= yieldLoss;
            resourceDto.YieldLoss = yieldLoss;

            // RAID LOSS
            //
            //
            var isRaided = rand.Next(100) <= miningConfig.RaidChance * 100;
            if (isRaided)
            {
                var raidLossPercent =
                    rand.NextDouble() *
                    (miningConfig.RaidLossMaximum - miningConfig.RaidLossMinimum) + miningConfig.RaidLossMinimum;
                var raidLossAmount = Math.Round(resourceYield * raidLossPercent, 2);
                resourceYield -= raidLossAmount;
                resourceDto.IsRaided = true;
                resourceDto.RaidLoss = raidLossAmount;
            }

            if (inventoryItem is null)
            {
                inventoryItem = new InventoryItem();
                inventoryItem.Inventory = playerInventory;
                inventoryItem.Item = resource;
                inventoryItem.Quantity += resourceYield;
                await _db.InventoryItems.AddAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += resourceYield;
                _db.InventoryItems.Update(inventoryItem);
            }

            resourceDto.Resource = resource.Name;
            resourceDto.Quantity = Math.Round(resourceYield, 2);
            miningRewardDto.Rewards.Add(resourceDto);
        }

        // Item drop ( 10% chance, not including item drop chances)
        //TODO 100%  DROP CHANCE RIGHT NOW
        var itemDropped = rand.Next(100) <= 100;
        if (itemDropped)
        {
            var dropGenerator = new DropGenerator.DropGenerator(_db);
            var itemDrop = await dropGenerator.GenerateDrop(playerInventory);
            if (itemDrop is not null)
            {
                miningRewardDto.ItemDropped = true;
                miningRewardDto.ItemDropName = itemDrop.Item.Name;
                miningRewardDto.ItemDropType = itemDrop.Item.Type;
                miningRewardDto.ItemDropQuantity = itemDrop.Quantity;
                miningRewardDto.ItemDropMintNumber = itemDrop.MintNumber;
                miningRewardDto.ItemDropFoil = itemDrop.Foil;
            }
            else
            {
                miningRewardDto.ItemDropped = false;
            }
        }

        //      This will determine if a player 'mined' that sector.  This is how they will be distributed
        //      If sector is unassigned and players roll hits, then assign sector to them and return in MiningReward() 
        //      object.  If not, mine like normal.
        if (harvester.Sector.Inventory is null)
        {
            // 1% chance
            var didMineSector = rand.Next(100) > 1;
            if (didMineSector)
            {
                //  3% chance for Mythic rarity
                //  10% chance for Rare rarity
                //  30% chance for Uncommon rarity
                //  70% chance for Common rarity
                var sectorRarity = rand.Next(100) switch
                {
                    <= 3 => "Mythic",
                    <= 10 => "Rare",
                    <= 30 => "Uncommon",
                    _ => "Common"
                };
                // Update harvester sector
                harvester.Sector.Inventory = playerInventory;
                harvester.Sector.ForSale = false;
                harvester.Sector.Rarity = sectorRarity;
                harvester.Sector.Faction = playerFaction.FactionName;
                // Update mining rewards
                miningRewardDto.MinedSector = true;
                miningRewardDto.MinedSectorName = harvester.Sector.Name;
                miningRewardDto.MinedSectorPlanet = harvester.Sector.Planet;
                miningRewardDto.MinedSectorRarity = sectorRarity;
            }
            else
            {
                miningRewardDto.MinedSector = false;
            }
        }

        _db.Update(harvester);
        if (sector.Inventory is not null)
            _db.Inventories.Update(sector.Inventory);
        await _db.SaveChangesAsync();
        return miningRewardDto;
    }

    public async Task<bool> EquipItem(Guid playerId, string itemName)
    {
        var harvester = await _db.Harvesters.FirstOrDefaultAsync(h => h.Account.PlayerId == playerId);
        var inventory = await _inventoryManager.GetByPlayerId(playerId);
        if (harvester is null || inventory is null) return false;
        var inventoryItem = inventory.Items
            .FirstOrDefault(i => i.Item?.Name == itemName);
        // Not Owned
        if (inventoryItem is null) return false;

        switch (inventoryItem.Item?.Type)
        {
            case "Harvester Tool":
            {
                if (harvester.Tool is not null)
                {
                    harvester.Tool.Equipped = false;
                }

                harvester.Tool = inventoryItem;
                inventoryItem.Equipped = true;
                break;
            }
            case "Harvester Weapon":
            {
                if (harvester.Weapon is not null)
                {
                    harvester.Weapon.Equipped = false;
                }

                harvester.Weapon = inventoryItem;
                inventoryItem.Equipped = true;
                break;
            }
            case "Harvester Protection":
            {
                if (harvester.Protection is not null)
                {
                    harvester.Protection.Equipped = false;
                }

                harvester.Protection = inventoryItem;
                inventoryItem.Equipped = true;
                break;
            }
        }

        harvester.OnCooldown = true;
        harvester.Cooldown = DateTime.UtcNow.AddMinutes(0.5);

        _db.Harvesters.Update(harvester);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateHarvester(Account account)
    {
        var harvester = new Harvester();
        harvester.Account = account;
        var sector = await _db.Sectors.FirstAsync(g => g.Name == "Unassigned");
        harvester.Sector = sector;
        await _db.Harvesters.AddAsync(harvester);
        return await _db.SaveChangesAsync() == 1;
    }

    private async Task<IEnumerable<GameItem>> GetResourceGameItems(string planetName)
    {
        var resourceNames = PlanetRepository.GetPlanetItems(planetName);
        return await _db.GameItems.Where(s => resourceNames.Contains(s.Name)).ToListAsync();
    }
}