using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Server.Core.Inventories;

namespace Server.Core.Sectors;

public class Sector
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Planet { get; set; } = null!;
    public string Rarity { get; set; } = null!;
    [Column(TypeName = "decimal(18,2)")]
    public double Commission { get; set; }
    public DateTime CommissionChangeTimestamp { get; set; } = DateTime.UtcNow.Subtract(TimeSpan.FromHours(48));
    public bool ForSale { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public double Price { get; set; }
    public string? Faction { get; set; } = null!;
    
    [JsonIgnore]
    public virtual Inventory? Inventory { get; set; } = null!;
}