using GameLib.Data.GameItems.Consumable;
using Microsoft.EntityFrameworkCore;
using Server.Core._misc;
using Server.Core.Accounts;
using Server.Core.Accounts.Entities;
using Server.Database;
using Server.Services.Effex;

namespace Server.Core.Inventories;

public class InventoryManager
{
    private readonly GameDbContext _db;
    private readonly IServiceProvider _serviceProvider;

    public InventoryManager(GameDbContext db, IServiceProvider serviceProvider)
    {
        _db = db;
        _serviceProvider = serviceProvider;
    }

    // Gets a players inventory, including all navigation properties: account, items, and sectors
    public async Task<Inventory?> GetByPlayerId(Guid playerId)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (account is null) return null;
        var inventory = await _db.Inventories
            .Include(i => i.Account)
            .Include(i => i.Items)
            .ThenInclude(i => i.Item)
            .Include(i => i.Sectors)
            .FirstOrDefaultAsync(i => i.Account == account);
        return inventory;
    }

    public async Task<bool> CreateInventory(Account account)
    {
        var inventory = new Inventory
        {
            Account = account
        };
        await _db.Inventories.AddAsync(inventory);
        return await _db.SaveChangesAsync() == 1;
    }

    public async Task<bool> AddItem(Inventory inventory, InventoryItem itemToAdd)
    {
        var exists = inventory.Items.FirstOrDefault(i => i.Item == itemToAdd.Item);
        if (exists is null || itemToAdd.Item.Unique)
        {
            itemToAdd.Inventory = inventory;
            await _db.InventoryItems.AddAsync(itemToAdd);
            return await _db.SaveChangesAsync() == 1;
        }

        exists.Quantity += itemToAdd.Quantity;
        _db.InventoryItems.Update(exists);
        return await _db.SaveChangesAsync() == 1;
    }

    public async Task<bool> AddGameItem(Inventory inventory, GameItem item, double amount)
    {
        var exists = inventory.Items.FirstOrDefault(i => i.Item == item);
        if (exists is null || item.Unique)
            return await CreateItem(inventory, item, amount);

        exists.Quantity += amount;
        _db.InventoryItems.Update(exists);
        return await _db.SaveChangesAsync() == 1;
    }

    private async Task<bool> CreateItem(Inventory inventory, GameItem item, double amount)
    {
        var inventoryItem = new InventoryItem();
        inventoryItem.Inventory = inventory;
        inventoryItem.Item = item;
        inventoryItem.Quantity += amount;
        inventoryItem.Foil = "Standard";
        if (item.Unique)
        {
            inventoryItem.MintNumber = item.Minted + 1;
        }
        else
        {
            inventoryItem.MintNumber = null;
        }

        item.Minted += 1;
        _db.GameItems.Update(item);
        await _db.InventoryItems.AddAsync(inventoryItem);
        return await _db.SaveChangesAsync() == 2;
    }

    // Uses a game item out of a players inventory
    // Adds a ConsumableEffect to a sector through Effex
    public async Task<Tuple<bool, string>> UseGameItemOnSector(string sectorName, string gameItemName, Guid playerId)
    {
        var inventory = await GetByPlayerId(playerId);
        if (inventory is null) return new Tuple<bool, string>(false, "Inventory not found.");
        var inventoryItem = inventory.Items.FirstOrDefault(i => i.Item.Name == gameItemName);
        if (inventoryItem is null) return new Tuple<bool, string>(false, "Item not in inventory.");

        if (inventoryItem.Quantity > 1)
            inventoryItem.Quantity -= 1;
        else
            inventory.Items.Remove(inventoryItem);

        var effect = ConsumableRepository.UseConsumable(inventoryItem.Item.Name);
        if (effect is null) return new Tuple<bool, string>(false, "Consumable not found.");

        var effex = _serviceProvider.GetRequiredService<IEffex>();
        var (effectIsAdded, errorMsg) = effex.AddEffect(sectorName, effect);

        if (effectIsAdded)
        {
            _db.Inventories.Update(inventory);
            await _db.SaveChangesAsync();
            return new Tuple<bool, string>(true, "Used consumable.");
        }

        return new Tuple<bool, string>(false, errorMsg);
    }
}