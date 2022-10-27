using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Server.Core._misc;

namespace Server.Core.Market.Entities;

public class MarketLogGameItemSale
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }
    public string BuyerUsername { get; set; } = null!;
    public string SellerUsername { get; set; } = null!;
    public DateTime Date { get; set; }
    [Column(TypeName = "decimal(18,2)")] public double Price { get; set; }
    [Column(TypeName = "decimal(18,2)")] public double Quantity { get; set; }
    public int? MintNumber { get; set; }
    public string? Foil { get; set; }
    public virtual GameItem GameItem { get; set; } = null!;
}