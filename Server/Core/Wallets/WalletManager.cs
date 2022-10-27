using Microsoft.EntityFrameworkCore;
using Server.Core.Accounts;
using Server.Core.Accounts.Entities;
using Server.Database;

namespace Server.Core.Wallets;

public class WalletManager
{
    private readonly GameDbContext _db;

    public WalletManager(GameDbContext db)
    {
        _db = db;
    }
    
    public async Task<Wallet> GetByPlayerId(Guid playerId)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.Account == account);
        return wallet!;
    }

    public async Task<Wallet> AddBalance(Wallet wallet, double amount)
    {
        wallet.Balance += amount;
        _db.Wallets.Update(wallet);
        await _db.SaveChangesAsync();
        return wallet;
    }
    public async Task<Wallet> RemoveBalance(Wallet wallet, double amount)
    {
        wallet.Balance -= amount;
        _db.Wallets.Update(wallet);
        await _db.SaveChangesAsync();
        return wallet;
    }

    public async Task<bool> CreateWallet(Account account)
    {
        var wallet = new Wallet();
        wallet.Account = account;
        wallet.Balance = 0;
        await _db.Wallets.AddAsync(wallet);

        var success = await _db.SaveChangesAsync();
        return success == 1;
    }

}