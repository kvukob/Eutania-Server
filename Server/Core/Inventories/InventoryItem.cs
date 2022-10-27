using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Server.Core._misc;
using Server.Core.Market;
using Server.Core.Market.Entities;

namespace Server.Core.Inventories;

public class InventoryItem : MarketItem
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }
    public Guid Identifier { get; set; } = Guid.NewGuid();

    [Column(TypeName = "decimal(18,2)")] 
    public double Quantity { get; set; }
    public bool? Equipped { get; set; }
    public int? MintNumber { get; set; }
    public string? Foil { get; set; } = null!;
    public virtual GameItem Item { get; set; } = null!;
    [JsonIgnore]
    public virtual Inventory Inventory { get; set; } = null!;
}