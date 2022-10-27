using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Server.Core.Accounts;
using Server.Core.Accounts.Entities;

namespace Server.Core._misc;

public class VerificationCode
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }

    public string Code { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
    public VerificationCodeType Type { get; set; }
    public virtual Account Account { get; set; } = null!;
}