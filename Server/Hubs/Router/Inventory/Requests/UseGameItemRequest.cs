namespace Server.Hubs.Router.Inventory.Requests;

public class UseGameItemRequest
{
    public string SectorName { get; set; } = null!;
    public string GameItemName { get; set; } = null!;
}