namespace Server.Hubs.Router.Sector.Requests;

public class ChangeCommissionRateRequest
{
    public string SectorName { get; set; } = null!;
    public double Rate { get; set; }
}