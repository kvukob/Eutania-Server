namespace Server.Hubs.Router.Initialize.Requests;

public class InitializeRequest
{
    public string Username { get; set; } = null!;
    public string Faction { get; set; } = null!;
}