using GameLib.Data.GameItems;
using Microsoft.EntityFrameworkCore;
using Server.Core.Inventories;
using Server.Database;

namespace Server.Core.Harvesters.DropGenerator;

public class DropGenerator
{
    private readonly GameDbContext _db;

    public DropGenerator(GameDbContext db)
    {
        _db = db;
    }

    // -> Generate random item type
    // -> Get a list of droppable game items from the database that match that item type
    // -> Generate quantity
    //      If it's a resource, generate between 0.01 and 0.25
    //      Any other items are 1 quantity
    // -> Generate drop chance percent minimum
    // -> Filter droppable items that have a drop chance higher than drop chance percent minimum
    //      e.g.  Drop Chance Range is 0.05
    //            Only items with a drop chance of 0.05 or higher are filtered
    // -> Pick a random item out of this pool

    public async Task<InventoryItem?> GenerateDrop(Inventory inventory)
    {
        var droppableTypes = GameItemRepository.GameItemTypes;
        var droppableTypesCount = droppableTypes.Count();

        var rng = new Random(Guid.NewGuid().GetHashCode());
        var randomType = rng.Next(droppableTypesCount);

        var randomItemType = droppableTypes.ElementAt(randomType);
        // All of the possible items that can drop of that item type
        var possibleItems = await _db.GameItems.Where(gI => gI.Type == randomItemType).ToListAsync();


        var dropChancePercentMinimum = rng.NextDouble();

        // Only items lower than the range can drop
        // Eg.  dropChancePercentMinimum = 0.03.  possibleItems is filtered down to items with greater than 0.03 drop chance
        possibleItems = possibleItems.Where(pI => pI.DropChance > dropChancePercentMinimum).ToList();


        foreach (var x in possibleItems)
        {
            Console.WriteLine(x.Name);
        }

        Console.WriteLine(possibleItems.Count + " POSSIBLE ITEMS COUNT");

        if (possibleItems.Count > 0)
        {
            var randomItem = rng.Next(possibleItems.Count);
            var gameItemDrop = possibleItems.ElementAt(randomItem);

            var iItemDrop = new InventoryItem();
            iItemDrop.Item = gameItemDrop;
            iItemDrop.Inventory = inventory;
            if (gameItemDrop.Unique)
            {
                iItemDrop.Foil = rng.Next(100) switch
                {
                    <= 1 => "Frost",
                    <= 5 => "Flame",
                    <= 10 => "Evil",
                    _ => "Standard"
                };
                iItemDrop.Quantity = 1;
                iItemDrop.MintNumber = gameItemDrop.Minted + 1;
                await _db.InventoryItems.AddAsync(iItemDrop);
                gameItemDrop.Minted++;
                _db.GameItems.Update(gameItemDrop);
            }
            else
            {
                var quantity = Math.Round(rng.NextDouble() * (0.25 - 0.01) + 0.01, 2);
                iItemDrop.Quantity = quantity;
                var existingItem = inventory.Items.FirstOrDefault(i => i.Item == gameItemDrop);
                if (existingItem is null)
                {
                    await _db.InventoryItems.AddAsync(iItemDrop);
                }
                else
                {
                    existingItem.Quantity += quantity;
                    _db.InventoryItems.Update(existingItem);
                }
            }

            await _db.SaveChangesAsync();

            return iItemDrop;
        }

        return null;
    }
}