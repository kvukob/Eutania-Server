namespace Server.Hubs.Router.Market.Requests;

public class SellListingRequest
{
    public string Identifier { get; set; } = null!;
    public string ItemName { get; set; } = null!;
    public double Quantity { get; set;}
    public double Price { get; set; }
}