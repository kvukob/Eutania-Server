namespace GameLib.Data.GameItems.Harvester;

public class HarvesterToolFeature
{
    public int MiningSpeed { get; set; }
    public string BonusResourceName { get; set; } = null!;
    public double BonusResourceFactor { get; set; }
}

internal class HarvesterToolFeatures
{
    public HarvesterToolFeature GetFeatures(string itemName)
    {
        return itemName switch
        {
            "Flame Clipper" => FlameClipper(),
            "Echoing Inferno" => EchoingInferno(),
            "Harmonious Bite" => HarmoniousBite(),
            "Archon's Light" => ArchonsLight(),
            "Guiding Lance" => GuidingLance(),
            _ => new HarvesterToolFeature()
        };
    }

    private HarvesterToolFeature FlameClipper()
    {
        return new HarvesterToolFeature()
        {
            MiningSpeed = -1,
            BonusResourceName = "",
            BonusResourceFactor = 1
        };
    }
    private HarvesterToolFeature EchoingInferno()
    {
        return new HarvesterToolFeature()
        {
            MiningSpeed = -2,
            BonusResourceName = "Avanthium",
            BonusResourceFactor = 1.05
        };
    }
    private HarvesterToolFeature HarmoniousBite()
    {
        return new HarvesterToolFeature()
        {
            MiningSpeed = -4,
            BonusResourceName = "Liavine",
            BonusResourceFactor = 1.05
        };
    }
    private HarvesterToolFeature ArchonsLight()  
    {
        return new HarvesterToolFeature()
        {
            MiningSpeed = -6,
            BonusResourceName = "Vohphos",
            BonusResourceFactor = 1.05
        };
    }
    private HarvesterToolFeature GuidingLance()
    {
        return new HarvesterToolFeature()
        {
            MiningSpeed = -8,
            BonusResourceName = "Sangunalt",
            BonusResourceFactor = 1.05
        };
    }

}