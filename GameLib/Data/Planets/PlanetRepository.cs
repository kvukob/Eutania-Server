namespace GameLib.Data.Planets;

public static class PlanetRepository
{
    public static IEnumerable<string> GetPlanetItems(string planetName)
    {
        IEnumerable<string> resourceNames = new List<string>();
        switch (planetName)
        {
            case "Aethea":
                resourceNames = PlanetItems.AetheaItems();
                break;
            case "Amaut":
                resourceNames = PlanetItems.AmautItems();
                break;
            case "Draecarra":
                resourceNames = PlanetItems.DraecarraItems();
                break;
            case "Eutania":
                resourceNames = PlanetItems.EutaniaItems();
                break;
            case "Luthien":
                resourceNames = PlanetItems.LuthienItems();
                break;
            case "Necyke":
                resourceNames = PlanetItems.NecykeItems();
                break;
            case "Psigen":
                resourceNames = PlanetItems.PsigenItems();
                break;
            case "Tsuma":
                resourceNames = PlanetItems.TsumaItems();
                break;
        }
        return resourceNames;
    }
    public static double GetMiningYield(string planetName, string resource)
    {
        return planetName switch
        {
            "Aethea" => MiningYields.AetheaYield(resource),
            "Amaut" => MiningYields.AmautYield(resource),
            "Draecarra" => MiningYields.DraecarraYield(resource),
            "Eutania" => MiningYields.EutaniaYield(resource),
            "Necyke" => MiningYields.NecykeYield(resource),
            "Luthien" => MiningYields.LuthienYield(resource),
            "Psigen" => MiningYields.PsigenYield(resource),
            "Tsuma" => MiningYields.TsumaYield(resource),
            _ => 0
        };
    }
}