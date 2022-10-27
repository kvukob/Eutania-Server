using Microsoft.EntityFrameworkCore;
using Server.Core._misc;
using Server.Core.Accounts;
using Server.Core.Accounts.Entities;
using Server.Core.Factions;
using Server.Core.Inventories;
using Server.Core.Market;
using Server.Core.Harvesters;
using Server.Core.Harvesters.Entities;
using Server.Core.Market.Entities;
using Server.Core.Mission;
using Server.Core.Sectors;
using Server.Core.Wallets;

namespace Server.Database;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; } = null!;

    public virtual DbSet<VerificationCode> VerificationCodes { get; set; } = null!;
    public virtual DbSet<Harvester> Harvesters { get; set; } = null!;
    public virtual DbSet<Inventory> Inventories { get; set; } = null!;
    public virtual DbSet<InventoryItem> InventoryItems { get; set; } = null!;
    public virtual DbSet<Wallet> Wallets { get; set; } = null!;
    
    public virtual DbSet<GameItem> GameItems { get; set; } = null!;
    public virtual DbSet<GameItemBurn> GameItemBurns { get; set; } = null!;
    public virtual DbSet<Sector> Sectors { get; set; } = null!;
    public virtual DbSet<Faction> Factions { get; set; } = null!;
    
    public virtual DbSet<Mission> Missions { get; set; } = null!;
    public virtual DbSet<MissionRequirement> MissionRequirements { get; set; } = null!;
    
    // Market
    public virtual DbSet<MarketListing> MarketListings { get; set; } = null!;
    public virtual DbSet<MarketListingItem> MarketListingItems { get; set; } = null!;
    public virtual DbSet<MarketLogGameItemSale> MarketLogGameItemSales { get; set; } = null!;
    public virtual DbSet<MarketLogSectorSale> MarketLogSectorSales { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       // modelBuilder.Entity<MarketListing>()
       //     .HasOne(f => f.InventoryItem)
       //     .WithMany()
        //    .OnDelete(DeleteBehavior.Restrict);
    }
}