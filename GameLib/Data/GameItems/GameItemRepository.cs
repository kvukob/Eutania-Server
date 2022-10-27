using GameLib.Data.GameItems.Harvester;

namespace GameLib.Data.GameItems;

public static class GameItemRepository
{
    public static readonly List<string> GameItemTypes = new()
    {
        "Resource",
        "Harvester Tool",
        "Harvester Weapon",
        "Harvester Protection",
        "Vintage",
    };
    public static HarvesterToolFeature GetHarvesterToolFeatures(string itemName)
    {
        return new HarvesterToolFeatures().GetFeatures(itemName);
    }

    public static HarvesterWeaponFeature GetHarvesterWeaponFeatures(string itemName)
    {
        return new HarvesterTurretFeatures().GetFeatures(itemName);
    }

    public static HarvesterProtectionFeature GetHarvesterProtectionFeatures(string itemName)
    {
        return new HarvesterProtectionFeatures().GetFeatures(itemName);
    }
}