using Server.Core.Wallets;
using Server.Database;
using Server.Web;

namespace Server.Hubs.Router.Wallet;

public class WalletRoute
{
    private readonly WalletManager _walletManager;

    public WalletRoute(GameDbContext db)
    {
        _walletManager = new WalletManager(db);
    }

    public async Task<HubResponse> Handle(string command, string request, Guid playerId)
    {
        return command switch
        {
            "getWallet" => await GetWallet(playerId),
            _ => new HubResponse()
        };
    }
    
    private async Task<HubResponse> GetWallet(Guid playerId)
    {
        var wallet = await _walletManager.GetByPlayerId(playerId);
        var response = new HubResponse()
        {
            Success = true,
            Data = new 
            {
                Balance = wallet.Balance
            }
        };
        return response;
    }
}