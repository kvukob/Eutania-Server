using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Core.Accounts;
using Server.Core.Accounts.Entities;
using Server.Database;
using Server.Hubs.Router;
using Server.Web;

namespace Server.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly HubRouter _router;
    private readonly GameDbContext _gameDb;

    public GameHub(GameDbContext db, IServiceProvider serviceProvider)
    {
        _router = new HubRouter(db, serviceProvider);
        _gameDb = db;
    }

    public async Task V1(string route, string command, string request)
    {
        var response = await _router.Route(route, command, request, this.GetPlayerId(Context));
        await Clients.Caller.SendAsync(command, response);
    }

    public async Task Login()
    {
        var playerId = this.GetPlayerId(Context);
        var account = await _gameDb.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (account is not null)
        {
            // Create character
            if (!account.Initialized)
            {
                await Clients.Caller.SendAsync("mustInitialize");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, playerId.ToString());

            var loadObject = await CreateLoadObject(account);
            
            await Clients.Caller.SendAsync("login", loadObject);
        }
    }

    private async Task<object> CreateLoadObject(Account account)
    {
        var faction = await _gameDb.Factions.FirstOrDefaultAsync(f => f.Account == account);
        var wallet = await _gameDb.Wallets.FirstOrDefaultAsync(w => w.Account == account);
        return new
        {
            Account = new
            {
                Username = account.Username,
            },
            Faction = new
            {
                Name = faction?.FactionName,
            },
            Wallet = new
            {
                Balance = wallet?.Balance,
            }
        };
    }


    // GET ACCOUNT IDENTIFIER
    // var playerId = this.GetPlayerId(Context).ToString();
    // GET ACCESS TOKEN
    // var accessToken = await Context.GetHttpContext().GetTokenAsync("access_token");
}