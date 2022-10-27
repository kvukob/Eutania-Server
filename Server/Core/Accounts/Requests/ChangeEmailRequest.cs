namespace Server.Core.Accounts.Requests;

public class ChangeEmailRequest
{
    public string NewEmail { get; set; } = null!;
}