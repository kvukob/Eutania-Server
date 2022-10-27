namespace Server.Core.Accounts.Requests;

public class AccountRecoveryRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Code { get; set; } = null!;
}