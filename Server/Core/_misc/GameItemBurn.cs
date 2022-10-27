using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Server.Core._misc;

public class GameItemBurn
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }
    public virtual GameItem GameItem { get; set; }
    public int? MintNumber { get; set; }
}