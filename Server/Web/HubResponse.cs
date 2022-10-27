namespace Server.Web;

public class HubResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

}