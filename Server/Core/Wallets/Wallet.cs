using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Server.Core.Accounts;
using Server.Core.Accounts.Entities;

namespace Server.Core.Wallets;

public class Wallet
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public double Balance { get; set; }
    [JsonIgnore]
    public virtual Account Account { get; set; } = null!;
}