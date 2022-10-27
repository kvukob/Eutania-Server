using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Server.Core.Sectors;

namespace Server.Core.Market.Entities;

public class MarketLogSectorSale
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }
    public string BuyerUsername { get; set; } = null!;
    public string SellerUsername { get; set; } = null!;
    public DateTime Date { get; set; }
    [Column(TypeName = "decimal(18,2)")] public double Price { get; set; }
    public virtual Sector Sector { get; set; } = null!;
}