namespace GameLib.Data.GameItems.Consumable;

public static class ConsumableRepository
{
    public static ConsumableEffect? UseConsumable(string consumableName)
    {
        switch (consumableName)
        {
            case "Basic Spectrometer":
                return BasicSpectrometer(consumableName);
            case "Raiding Drone":
                return RaidingDrone(consumableName);
            case "Accelerator Module":
                return AcceleratorModule(consumableName);
        }
        return null;
    }

    private static ConsumableEffect BasicSpectrometer(string consumableName)
    {
        return new ConsumableEffect()
        {
            GameItemName = consumableName,
            Type = "MiningYield",
            Modifier = 1.05,
            Charges = 50,
            IsBuff = true,
        };
    }

    private static ConsumableEffect RaidingDrone(string consumableName)
    {
        return new ConsumableEffect()
        {
            GameItemName = consumableName,
            Type = "RaidChance",
            Modifier = 1.05,
            Charges = 50,
            IsBuff = false,
        };
    }

    private static ConsumableEffect AcceleratorModule(string consumableName)
    {
        return new ConsumableEffect()
        {
            GameItemName = consumableName,
            Type = "MiningSpeed",
            Modifier = -10,
            Charges = 50,
            IsBuff = true,
        };
    }
}