namespace Server.Hubs.Router.Market.Requests;

public class SellSectorRequest
{
    public string SectorName { get; set; } = null!;
    public double Price { get; set; }
}