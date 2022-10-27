using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Server.Core._misc;

public class GameItem
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool Unique { get; set; }

    [JsonIgnore] public int Minted { get; set; } = 0;
    [JsonIgnore]
    public double DropChance { get; set; }
}