using GameLib.Data.GameItems;
using GameLib.Data.GameItems.Consumable;
using Server.Core.Harvesters.Entities;
using Server.Services.Effex;

namespace Server.Core.Harvesters.DropGenerator;

public class MiningConfiguration
{
    private const int MiningCooldown = 60;
    public double YieldLossMinimum { get; set; } = 0.05;
    public double YieldLossMaximum { get; set; } = 0.40;
    public double RaidChance { get; set; } = 0.50;
    public double RaidLossMinimum { get; set; } = 0.05;
    public double RaidLossMaximum { get; set; } = 0.25;
    public string BonusResourceName { get; set; } = null!;
    public double BonusResourceFactor { get; set; } = 1.0;

    public Tuple<MiningConfiguration, Harvester> Configure(IEffex effex, Harvester harvester,
        List<ConsumableEffect> consumableEffects)
    {
        harvester.Cooldown = DateTime.UtcNow.Add(TimeSpan.FromSeconds(MiningCooldown));

        if (harvester.Tool is not null)
        {
            var feature = GameItemRepository.GetHarvesterToolFeatures(harvester.Tool.Item.Name);
            harvester.Cooldown = harvester.Cooldown.Add(TimeSpan.FromSeconds(feature.MiningSpeed));
            BonusResourceName = feature.BonusResourceName;
            BonusResourceFactor = feature.BonusResourceFactor;
        }

        if (harvester.Weapon is not null)
        {
            var feature = GameItemRepository.GetHarvesterWeaponFeatures(harvester.Weapon.Item.Name);
            RaidChance += feature.RaidChance;
        }

        if (harvester.Protection is not null)
        {
            var feature = GameItemRepository.GetHarvesterProtectionFeatures(harvester.Protection.Item.Name);
            RaidLossMaximum += feature.RaidLoss;
        }

        // Sector effects
        // harvester has sector property for all needed data

        harvester.OnCooldown = true;
        var miningSpeedEffects = consumableEffects
            .Where(e => e.Type == "MiningSpeed");
        
        // Cycles through all effects applied to a sector and applies any that relate to mining speed
        foreach (var effect in miningSpeedEffects)
        {
            var effectModifier = ConsumableRepository.UseConsumable(effect.GameItemName);
            if (effectModifier is not null)
            {
                harvester.Cooldown = harvester.Cooldown.Add(TimeSpan.FromSeconds(effectModifier.Modifier));
                effex.UseEffect(harvester.Sector.Name, effect);
            }
        }

        return new Tuple<MiningConfiguration, Harvester>(this, harvester);
    }
}