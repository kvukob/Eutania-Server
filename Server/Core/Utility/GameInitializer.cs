using GameLib.Data.Factions;
using Server.Core.Accounts;
using Server.Core.Factions;
using Server.Core.Harvesters;
using Server.Core.Inventories;
using Server.Core.Wallets;
using Server.Database;
using Server.Hubs.Router.Initialize.Requests;

namespace Server.Core.Utility;

public class GameInitializer
{
    private readonly AccountManager _accountManager;
    private readonly HarvesterManager _harvesterManager;
    private readonly FactionManager _factionManager;
    private readonly InventoryManager _inventoryManager;
    private readonly WalletManager _walletManager;

    public GameInitializer(GameDbContext gameDb, IServiceProvider serviceProvider)
    {
        _accountManager = new AccountManager(gameDb);
        _harvesterManager = new HarvesterManager(gameDb, serviceProvider);
        _factionManager = new FactionManager(gameDb);
        _inventoryManager = new InventoryManager(gameDb, serviceProvider);
        _walletManager = new WalletManager(gameDb);
    }

    public async Task<Tuple<bool, string>> InitializeGame(Guid playerId, InitializeRequest request)
    {
        var account = await _accountManager.GetByPlayerId(playerId);
        if (account is null) return new Tuple<bool, string>(false, "Account does not exist to initialize.");
        
        // Harvester
        var initialized = await _harvesterManager.CreateHarvester(account);
        if (!initialized) return new Tuple<bool, string>(false, "Error initializing harvester.");
        
        // Faction
        if (!GameFaction.IsValidFaction(request.Faction))
            return new Tuple<bool, string>(false, "Invalid faction.");
        initialized = await _factionManager.AssignFaction(playerId, request.Faction);
        if (!initialized) return new Tuple<bool, string>(false, "Error initializing faction.");
        
        // Inventory
        initialized = await _inventoryManager.CreateInventory(account);
        if (!initialized) return new Tuple<bool, string>(false, "Error initializing inventory.");
        
        // Wallet
        initialized = await _walletManager.CreateWallet(account);
        if (!initialized) return new Tuple<bool, string>(false, "Error initializing wallet.");

        // Account
        initialized = await _accountManager.Initialize(playerId, request.Username);
        if (!initialized) return new Tuple<bool, string>(false, "Error initializing account.");

        return new Tuple<bool, string>(initialized, "");
    }
}