namespace GameLib.Data.GameItems.Harvester;

public class HarvesterWeaponFeature
{
    public double RaidChance { get; set; }
}

internal class HarvesterTurretFeatures
{
    public HarvesterWeaponFeature GetFeatures(string itemName)
    {
        return itemName switch
        {
            "Packrat Cannon" => PackratCannon(),
            "Star-Volt Zapper" => StarVoltZapper(),
            "Light Projector" => LightProjector(),
            _ => new HarvesterWeaponFeature()
        };
    }

    private HarvesterWeaponFeature PackratCannon()
    {
        return new HarvesterWeaponFeature()
        {
            RaidChance = -0.01,
        };
    }
    private HarvesterWeaponFeature StarVoltZapper()
    {
        return new HarvesterWeaponFeature()
        {
            RaidChance = -0.03,
        };
    }    
    
    private HarvesterWeaponFeature LightProjector()
    {
        return new HarvesterWeaponFeature()
        {
            RaidChance = -0.05,
        };
    }
}