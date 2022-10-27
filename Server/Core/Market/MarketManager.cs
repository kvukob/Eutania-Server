using Microsoft.EntityFrameworkCore;
using PagedList;
using Server.Core.Inventories;
using Server.Core.Market.Entities;
using Server.Core.Sectors;
using Server.Core.Wallets;
using Server.Database;
using Server.Services.Satellite;

namespace Server.Core.Market;

public class MarketManager
{
    private readonly GameDbContext _db;
    private readonly WalletManager _walletManager;
    private readonly InventoryManager _inventoryManager;
    private readonly IServiceProvider _serviceProvider;

    public MarketManager(GameDbContext db, IServiceProvider serviceProvider)
    {
        _db = db;
        _walletManager = new WalletManager(db);
        _inventoryManager = new InventoryManager(db, serviceProvider);
        _serviceProvider = serviceProvider;
    }

    #region Sectors

    public async Task<IEnumerable<Sector>> GetPlayerSectors(Guid playerId)
    {
        var playerInventory = await _inventoryManager.GetByPlayerId(playerId);
        var x = await _db.Sectors.Where(s => s.Inventory == playerInventory && s.ForSale).ToListAsync();

        return x;
    }

    public async Task<Tuple<IEnumerable<object>, int>> GetSectors(int page, string? planet, string? rarity)
    {
        var x = await _db.Sectors
            .Include(i => i.Inventory)
            .Select(s => new
            {
                Sector = s,
                Owner = s.Inventory == null ? null : s.Inventory.Account.Username
            })
            .Where(s => s.Sector.ForSale == true)
            .Where(s => s.Sector.Planet.ToLower() == (planet ?? s.Sector.Planet))
            .Where(s => s.Sector.Rarity.ToLower() == (rarity ?? s.Sector.Rarity))
            .ToListAsync();

        return new Tuple<IEnumerable<object>, int>(x.ToPagedList(page, 8), x.Count);
    }

    public async Task<bool> BuySector(Guid playerId, string sectorName)
    {
        var buyerInventory = await _inventoryManager.GetByPlayerId(playerId);
        var wallet = await _db.Wallets
            .Include(w => w.Account)
            .FirstOrDefaultAsync(w => w.Account.PlayerId == playerId);
        var sector = await _db.Sectors
            .Where(s => s.Inventory != null)
            .Include(s => s.Inventory)
            .ThenInclude(sI => sI!.Account)
            .FirstOrDefaultAsync(s => s.Name == sectorName);
        var playerFaction = await _db.Factions.FirstOrDefaultAsync(f => f.Account.PlayerId == playerId);
        if (buyerInventory is null || wallet is null || sector is null || playerFaction is null)
            return false;

        if (!sector.ForSale) return false;

        if (wallet.Balance < sector.Price) return false;

        wallet.Balance -= sector.Price;
        _db.Wallets.Update(wallet);

        await LogSectorSale(sector, wallet.Account.Username ?? "", sector.Inventory?.Account.Username ?? "");

        sector.ForSale = false;
        sector.Price = -1;
        sector.CommissionChangeTimestamp = DateTime.UtcNow.Subtract(TimeSpan.FromHours(48));
        sector.Faction = playerFaction.FactionName;
        sector.Inventory = buyerInventory;
        _db.Sectors.Update(sector);

        return await _db.SaveChangesAsync() == 3;
    }

    public async Task<bool> SellSector(Guid playerId, string sectorName, double price)
    {
        var ownerInventory = await _inventoryManager.GetByPlayerId(playerId);
        var sector = await _db.Sectors.FirstOrDefaultAsync(s => s.Name == sectorName);
        if (ownerInventory is null || sector is null)
            return false;
        // Not owned by player
        if (ownerInventory != sector.Inventory) return false;
        if (price <= 0) return false;

        sector.ForSale = true;
        sector.Price = price;

        _db.Sectors.Update(sector);
        return await _db.SaveChangesAsync() == 1;
    }

    public async Task<bool> CancelSector(Guid playerId, string sectorName)
    {
        var ownerInventory = await _inventoryManager.GetByPlayerId(playerId);
        var sector = await _db.Sectors.FirstOrDefaultAsync(s => s.Name == sectorName);
        if (ownerInventory is null || sector is null)
            return false;
        // Not owned by player
        if (ownerInventory != sector.Inventory) return false;

        sector.ForSale = false;
        sector.Price = -1;

        _db.Sectors.Update(sector);
        await _db.SaveChangesAsync();
        return true;
    }

    #endregion

    public async Task<Tuple<IEnumerable<object>, int>> GetLatest()
    {
        var listings = await _db.MarketListings
            .Include(l => l.Item)
            .Include(l => l.Seller)
            .OrderByDescending(l => l.Id)
            .Take(8)
            .Select(l => new
            {
                Item = new
                {
                    Name = l.Item.GameItem.Name,
                    Type = l.Item.GameItem.Type,
                    MintNumber = l.Item.MintNumber,
                    Foil = l.Item.Foil
                },
                Id = l.Id,
                Quantity = l.Quantity,
                Price = l.Price,
                Seller = l.Seller
            })
            .ToListAsync();
        return new Tuple<IEnumerable<object>, int>(listings, listings.Count);
    }

    public async Task<MarketListing?> GetById(int id)
    {
        return await _db.MarketListings
            .Include(l => l.Item)
            .ThenInclude(i => i.GameItem)
            .Include(l => l.Seller)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<object>> GetBySelfListed(Guid playerId)
    {
        return await _db.MarketListings
            .Include(l => l.Item)
            .ThenInclude(i => i.GameItem)
            .Where(l => l.Seller.PlayerId == playerId)
            .Select(l => new
            {
                Id = l.Id,
                Name = l.Item.GameItem.Name,
                Type = l.Item.GameItem.Type,
                Price = l.Price,
                Quantity = l.Quantity,
                MintNumber = l.Item.MintNumber,
                Foil = l.Item.Foil
            })
            .ToListAsync();
    }

    public async Task<Tuple<IEnumerable<object>, int>> GetListingsByType(int page, string type)
    {
        var listings =
            await _db.MarketListings
                .Include(l => l.Item)
                .Include(l => l.Seller)
                .Where(l => l.Item.GameItem.Type.ToLower() == type.ToLower())
                .Select(l => new
                {
                    Item = new
                    {
                        Name = l.Item.GameItem.Name,
                        Type = l.Item.GameItem.Type,
                        MintNumber = l.Item.MintNumber,
                        Foil = l.Item.Foil
                    },
                    Id = l.Id,
                    Quantity = l.Quantity,
                    Price = l.Price,
                    Seller = l.Seller
                })
                .ToListAsync();
        return new Tuple<IEnumerable<object>, int>(listings.ToPagedList(page, 8), listings.Count);
    }

    public async Task<Tuple<IEnumerable<object>, int>> GetListingsByItemName(int page, string name)
    {
        var listings =
            await _db.MarketListings
                .Include(l => l.Item)
                .ThenInclude(i => i.GameItem)
                .Include(l => l.Seller)
                .Where(l => l.Item.GameItem.Name.ToLower() == name.ToLower())
                .Select(l => new
                {
                    Item = new
                    {
                        Name = l.Item.GameItem.Name,
                        Type = l.Item.GameItem.Type,
                        MintNumber = l.Item.MintNumber,
                        Foil = l.Item.Foil
                    },
                    Id = l.Id,
                    Quantity = l.Quantity,
                    Price = l.Price,
                    Seller = l.Seller
                })
                .ToListAsync();
        return new Tuple<IEnumerable<object>, int>(listings.ToPagedList(page, 8), listings.Count);
    }

    public async Task<string?> BuyListing(Guid playerId, int listingId)
    {
        var buyerWallet = await _walletManager.GetByPlayerId(playerId);
        var buyerInventory = await _inventoryManager.GetByPlayerId(playerId);
        if (buyerInventory is null) return null;

        var listing = await _db.MarketListings
            .Include(l => l.Item)
            .ThenInclude(i => i.GameItem)
            .Include(l => l.Seller)
            .FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing is null) return null;

        // Can't buy own listings
        if (buyerInventory.Account.PlayerId == listing.Seller.PlayerId) return null;

        var marketFee = 0.04; // 4%

        var feeAmount = (listing.Price * listing.Quantity) * marketFee;

        // Update buyers wallet
        var buyerCostBasis = (listing.Price * listing.Quantity) + feeAmount;
        if (buyerWallet.Balance < buyerCostBasis) return null;
        buyerWallet = await _walletManager.RemoveBalance(buyerWallet, buyerCostBasis);

        // Update sellers wallet
        var sellerWallet = await _walletManager.GetByPlayerId(listing.Seller.PlayerId);
        var sellerReward = (listing.Price * listing.Quantity) - feeAmount;
        sellerWallet = await _walletManager.AddBalance(sellerWallet, sellerReward);

        var boughtItem = new InventoryItem()
        {
            Equipped = false,
            MintNumber = listing.Item.MintNumber,
            Foil = listing.Item.Foil,
            Item = listing.Item.GameItem,
            Inventory = buyerInventory,
            Quantity = listing.Quantity
        };

        var receivedItem = await _db.InventoryItems.AddAsync(boughtItem);

        await LogGameItemSale(listing, buyerInventory.Account.Username ?? "", listing.Seller.Username ?? "");

        _db.MarketListings.Remove(listing);
        _db.MarketListingItems.Remove(listing.Item);
        await _db.SaveChangesAsync();

        // Send message to seller if they are online
        var x = _serviceProvider.GetRequiredService<IHubSatellite>();
        await x.SendToClient(
            listing.Seller.PlayerId.ToString(),
            "serverNotify",
            $"Your listing for {listing.Item.GameItem.Name} x{listing.Quantity} has sold on the market.");

        return $"Bought {listing.Quantity} {listing.Item.GameItem.Name}";
    }

    public async Task<string?> SellListing(Guid playerId, string itemIdentifier, double quantity, double price)
    {
        var seller = await _db.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (seller is null) return null;
        var inventory = await _inventoryManager.GetByPlayerId(playerId);
        if (inventory is null) return null;
        var itemToSell = inventory.Items.FirstOrDefault(i => i.Identifier == Guid.Parse(itemIdentifier));
        if (itemToSell is null) return null; // Don't own item
        if (itemToSell.Quantity < quantity) return null; // Not enough of item
        if (itemToSell.Equipped == true) return null; // Item is equipped

        var marketListingItem = new MarketListingItem()
        {
            GameItem = itemToSell.Item,
            MintNumber = itemToSell.MintNumber,
            Foil = itemToSell.Foil
        };
        await _db.MarketListingItems.AddAsync(marketListingItem);
        var listing = new MarketListing
        {
            Price = price,
            Quantity = quantity,
            Seller = seller,
            Item = marketListingItem
        };
        await _db.MarketListings.AddAsync(listing);

        if (itemToSell.Item.Unique)
        {
            _db.InventoryItems.Remove(itemToSell);
        }
        else
        {
            // Remove quantity from inventory
            itemToSell.Quantity -= quantity;
        }

        _db.Inventories.Update(inventory);
        await _db.SaveChangesAsync();

        return $"Listed.";
    }

    public async Task<Tuple<bool, string>> CancelListing(Guid playerId, int listingId)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        var inventory = await _inventoryManager.GetByPlayerId(playerId);
        var listing = await _db.MarketListings
            .Include(l => l.Item)
            .ThenInclude(l => l.GameItem)
            .Include(l => l.Seller)
            .FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing is null || account is null || inventory is null)
            return new Tuple<bool, string>(false, "Problem finding entities.");
        if (listing.Seller.PlayerId != account.PlayerId)
            return new Tuple<bool, string>(false, "Not listing owner.");

        if (!listing.Item.GameItem.Unique)
        {
            var existingItem = inventory.Items.First(i => i.Item.Name == listing.Item.GameItem.Name);
            existingItem.Quantity += existingItem.Quantity;
            _db.InventoryItems.Update(existingItem);
        }
        else
        {
            var itemToReturn = new InventoryItem()
            {
                Equipped = false,
                Inventory = inventory,
                Item = listing.Item.GameItem,
                MintNumber = listing.Item.MintNumber,
                Foil = listing.Item.Foil,
                Quantity = listing.Quantity
            };
            await _db.InventoryItems.AddAsync(itemToReturn);
        }

        _db.MarketListings.Remove(listing);
        await _db.SaveChangesAsync();
        return new Tuple<bool, string>(true, $"Cancelled listing for {listing.Item.GameItem.Name}.");
    }

    private async Task LogSectorSale(Sector sector, string buyerUsername, string sellerUsername)
    {
        var logItem = new MarketLogSectorSale()
        {
            BuyerUsername = buyerUsername,
            SellerUsername = sellerUsername,
            Date = DateTime.UtcNow,
            Price = sector.Price,
            Sector = sector
        };
        await _db.MarketLogSectorSales.AddAsync(logItem);
    }

    private async Task LogGameItemSale(MarketListing listing, string buyerUsername,
        string sellerUsername)
    {
        var logItem = new MarketLogGameItemSale()
        {
            BuyerUsername = buyerUsername,
            SellerUsername = sellerUsername,
            Date = DateTime.UtcNow,
            Price = listing.Price,
            Quantity = listing.Quantity,
            MintNumber = listing.Item.MintNumber,
            Foil = listing.Item.Foil,
            GameItem = listing.Item.GameItem
        };
        await _db.MarketLogGameItemSales.AddAsync(logItem);
    }
}