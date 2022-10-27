using System.ComponentModel.DataAnnotations.Schema;
using Server.Core.Accounts.Entities;

namespace Server.Core.Market.Entities;

public class MarketListing : MarketItem
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public double Price { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public double Quantity { get; set; }
    public virtual Account Seller { get; set; } = null!;
    public virtual MarketListingItem Item { get; set; } = null!;
}