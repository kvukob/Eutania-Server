namespace Server.Core.Harvesters.Models;

public class MiningRewardDto
{
    public List<ResourceReward> Rewards { get; set; } = new();
    public bool MinedSector { get; set; }
    public string MinedSectorName { get; set; } = null!;
    public string MinedSectorPlanet { get; set; } = null!;
    public string MinedSectorRarity { get; set; } = null!;

    public bool ItemDropped { get; set; }
    public string ItemDropName { get; set; } = null!;
    public string ItemDropType { get; set; } = null!;
    public double ItemDropQuantity { get; set; }
    public int? ItemDropMintNumber { get; set; }
    public string? ItemDropFoil { get; set; } = null!;
}