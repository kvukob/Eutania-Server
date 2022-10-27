namespace GameLib.Data.GameItems.Engineering;

public static class RecipeRepository
{
    public static GameItemRecipe GetRecipe(string itemName)
    {
        return itemName switch
        {
            "Insulated Wiring" => InsulatedWiring(),
            "Fused Parts" => FusedParts(),
            "Charged Core" => ChargedCore(),
            "Reactive Disc" => ReactiveDisc(),
            "Orythium Casing" => OrythiumCasing(),
            "Basic Spectrometer" => BasicSpectrometer(),
            "Raiding Drone" => RaidingDrone(),
            "Accelerator Module" => AcceleratorModule(),
            _ => new GameItemRecipe()
        };
    }

    private static GameItemRecipe InsulatedWiring()
    {
        return new GameItemRecipe()
        {
            RecipeItems = new List<RecipeItem>()
            {
                new RecipeItem(){ GameItemName = "Caelyrium", Quantity = 2},
                new RecipeItem(){ GameItemName = "Phytemicium", Quantity = 1.5}
            }
        };
    }
    private static GameItemRecipe FusedParts()
    {
        return new GameItemRecipe()
        {
            RecipeItems = new List<RecipeItem>()
            {
                new RecipeItem(){ GameItemName = "Avanthium", Quantity = 1},
                new RecipeItem(){ GameItemName = "Liavine", Quantity = 3},
                new RecipeItem(){ GameItemName = "Murissine", Quantity = 1}
            }
        };
    }
    private static GameItemRecipe ChargedCore()
    {
        return new GameItemRecipe()
        {
            RecipeItems = new List<RecipeItem>()
            {
                new RecipeItem(){ GameItemName = "Insulated Wiring", Quantity = 2},
                new RecipeItem(){ GameItemName = "Fused Parts", Quantity = 2},
                new RecipeItem(){ GameItemName = "Vohphos", Quantity = 3}
            }
        };
    }
    private static GameItemRecipe ReactiveDisc()
    {
        return new GameItemRecipe()
        {
            RecipeItems = new List<RecipeItem>()
            {
                new RecipeItem(){ GameItemName = "Insulated Wiring", Quantity = 2},
                new RecipeItem(){ GameItemName = "Fused Parts", Quantity = 2},
                new RecipeItem(){ GameItemName = "Mythine", Quantity = 1}
            }
        };
    }
    private static GameItemRecipe OrythiumCasing()
    {
        return new GameItemRecipe()
        {
            RecipeItems = new List<RecipeItem>()
            {
                new RecipeItem(){ GameItemName = "Fused Parts", Quantity = 1},
                new RecipeItem(){ GameItemName = "Orythium", Quantity = 5}
            }
        };
    }
    private static GameItemRecipe BasicSpectrometer()
    {
        return new GameItemRecipe()
        {
            RecipeItems = new List<RecipeItem>()
            {
                new RecipeItem(){ GameItemName = "Reactive Disc", Quantity = 1},
                new RecipeItem(){ GameItemName = "Orythium Casing", Quantity = 1}
            }
        };
    }
    private static GameItemRecipe RaidingDrone()
    {
        return new GameItemRecipe()
        {
            RecipeItems = new List<RecipeItem>()
            {
                new RecipeItem(){ GameItemName = "Insulated Wiring", Quantity = 5},
                new RecipeItem(){ GameItemName = "Fused Parts", Quantity = 5},
                new RecipeItem(){ GameItemName = "Reactive Disc", Quantity = 1},
                new RecipeItem(){ GameItemName = "Orythium Casing", Quantity = 3},
            }
        };
    }
    private static GameItemRecipe AcceleratorModule()
    {
        return new GameItemRecipe()
        {
            RecipeItems = new List<RecipeItem>()
            {
                new RecipeItem(){ GameItemName = "Insulated Wiring", Quantity = 5},
                new RecipeItem(){ GameItemName = "Fused Parts", Quantity = 5},
                new RecipeItem(){ GameItemName = "Charged Core", Quantity = 2},
            }
        };
    }
}