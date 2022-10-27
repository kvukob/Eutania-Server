using GameLib.Data.GameItems.Engineering;
using Server.Core.Inventories;
using Server.Database;

namespace Server.Core.Engineering;

public class EngineeringManager
{
    private readonly InventoryManager _inventoryManager;
    private readonly GameDbContext _db;

    public EngineeringManager(GameDbContext db, IServiceProvider serviceProvider)
    {
        _inventoryManager = new InventoryManager(db, serviceProvider);
        _db = db;
    }

    public async Task<Tuple<bool, string>> CraftItem(Guid playerId, string itemToCraftName)
    {
        var inventory = await _inventoryManager.GetByPlayerId(playerId);
        if (inventory is null) return new Tuple<bool, string>(false, "Inventory not found.");

        var gameItem = _db.GameItems.FirstOrDefault(gI => gI.Name == itemToCraftName);
        if (gameItem is null) return new Tuple<bool, string>(false, "GameItem not found.");
        ;
        var recipe = RecipeRepository.GetRecipe(gameItem.Name);

        foreach (var recipeItem in recipe)
        {
            var inventoryItem = inventory.Items.FirstOrDefault(i => i.Item.Name == recipeItem.GameItemName);
            if (inventoryItem is null || inventoryItem.Quantity < recipeItem.Quantity)
                return new Tuple<bool, string>(false, "Missing recipe requirements.");

            inventoryItem.Quantity -= recipeItem.Quantity;
        }

        var existingItem = inventory.Items.FirstOrDefault(ii => ii.Item.Name == itemToCraftName);

        if (gameItem.Unique || existingItem is null)
        {
            var craftedItem = new InventoryItem()
            {
                Inventory = inventory,
                Item = gameItem,
                Quantity = 1,
            };

            inventory.Items.Add(craftedItem);
        }
        else
        {
            existingItem.Quantity += 1;
        }

        _db.Inventories.Update(inventory);
        await _db.SaveChangesAsync();
        return new Tuple<bool, string>(true, $"Crafted {gameItem.Name}.");
    }
}