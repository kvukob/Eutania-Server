using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using GameLib.Data.Factions;
using Server.Core.Accounts;
using Server.Core.Accounts.Entities;

namespace Server.Core.Factions;



public class Faction
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }

    public string FactionName { get; set; } = GameFaction.Unassigned;
    [JsonIgnore]

    public virtual Account Account { get; set; } = null!;
}