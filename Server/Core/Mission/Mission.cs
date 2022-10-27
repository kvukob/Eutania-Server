using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Server.Core.Mission;

public class Mission
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string MissionCode { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Rarity { get; set; } = null!;
    public double CompletionCount { get; set; }
    public double MaxCompletions { get; set; }
    [JsonIgnore]
    public bool Active { get; set; }
    public double Reward { get; set; }
    public virtual ICollection<MissionRequirement> Requirements { get; set; } = null!;
}