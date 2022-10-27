namespace Server.Core.Accounts.Requests;

public class CreateAccountRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}