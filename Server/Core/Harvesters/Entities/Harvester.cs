using System.ComponentModel.DataAnnotations.Schema;
using Server.Core.Accounts.Entities;
using Server.Core.Inventories;
using Server.Core.Sectors;

namespace Server.Core.Harvesters.Entities;

public class Harvester
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public bool OnCooldown { get; set; }
    public DateTime Cooldown { get; set; }
    public virtual InventoryItem? Tool { get; set; }
    public virtual InventoryItem? Weapon { get; set; }
    public virtual InventoryItem? Protection { get; set; }
    public virtual Sector Sector { get; set; }
    public virtual Account Account { get; set; } = null!;

    public Harvester()
    {
        OnCooldown = false;
        Cooldown = DateTime.UtcNow;
    }
}