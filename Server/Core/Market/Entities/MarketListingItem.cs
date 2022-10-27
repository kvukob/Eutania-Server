using System.ComponentModel.DataAnnotations.Schema;
using Server.Core._misc;

namespace Server.Core.Market.Entities;

public class MarketListingItem
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public virtual GameItem GameItem { get; set; } = null!;
    
    // InventoryItem properties to be carried over
    public int? MintNumber { get; set; }
    public string? Foil { get; set; }
}