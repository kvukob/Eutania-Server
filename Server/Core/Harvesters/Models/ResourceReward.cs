namespace Server.Core.Harvesters.Models;

public class ResourceReward
{
    public string Resource { get; set; } = null!;
    public double Quantity { get; set; }
    public bool Bonus { get; set; }
    public double BonusQuantity { get; set; }
    public double YieldLoss { get; set; }
    public bool IsRaided { get; set; }
    public double RaidLoss { get; set; }
    public double Commission { get; set; }
}