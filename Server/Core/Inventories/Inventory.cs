using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Server.Core.Accounts;
using Server.Core.Accounts.Entities;
using Server.Core.Sectors;

namespace Server.Core.Inventories;

public class Inventory
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }
    public virtual Account Account { get; set; } = null!;
    public virtual ICollection<InventoryItem> Items { get; set; } = null!;
    public virtual ICollection<Sector> Sectors { get; set; } = null!;
}