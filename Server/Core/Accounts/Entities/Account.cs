using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Server.Core.Accounts.Entities;

public class Account
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public int Id { get; set; }
    [JsonIgnore]
    public Guid PlayerId { get; private set; } = Guid.NewGuid();
    [JsonIgnore]
    public string Email { get; set; } = null!;
    public string? Username { get; set; } = null!;
    [JsonIgnore]
    public string HashedPassword { get; set; } = null!;
    [JsonIgnore]
    public bool EmailVerified { get; set; } = false;
    [JsonIgnore]
    public DateTime Created { get; private set; } = DateTime.UtcNow;

    [JsonIgnore]
    public bool Initialized { get; set; } = false;

}