using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Server.Core._misc;

namespace Server.Core.Mission;

public class MissionRequirement
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }

    public double Quantity { get; set; }
    public virtual GameItem Item { get; set; } = null!;
    [JsonIgnore]
    public virtual Mission Mission { get; set; } = null!;
}