namespace GameLib.Data.GameItems.Harvester;

public class HarvesterProtectionFeature
{
    public double RaidLoss { get; set; }
}

internal class HarvesterProtectionFeatures
{
    public HarvesterProtectionFeature GetFeatures(string itemName)
    {
        return itemName switch
        {
            "Standard Shielding" => StandardShielding(),
            "Orythium Shielding" => OrythiumShielding(),
            "Phytemicium Shielding" => PhytemiciumShielding(),
            "Mythine Shielding" => MythineShielding(),
            _ => new HarvesterProtectionFeature()
        };
    }

    private HarvesterProtectionFeature StandardShielding()
    {
        return new HarvesterProtectionFeature()
        {
            RaidLoss = -0.01,
        };
    }
    private HarvesterProtectionFeature OrythiumShielding()
    {
        return new HarvesterProtectionFeature()
        {
            RaidLoss = -0.03,
        };
    }
    private HarvesterProtectionFeature PhytemiciumShielding()
    {
        return new HarvesterProtectionFeature()
        {
            RaidLoss = -0.05,
        };
    }
    private HarvesterProtectionFeature MythineShielding()
    {
        return new HarvesterProtectionFeature()
        {
            RaidLoss = -0.07,
        };
    }
}