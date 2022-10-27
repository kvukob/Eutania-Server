namespace GameLib.Data.Planets;

internal static class PlanetItems
{
    public static IEnumerable<string> AetheaItems() =>
        new List<string>()
        {
            "Murissine", "Orythium", "Phytemicium", "Tausmium",
        };

    public static IEnumerable<string> AmautItems() =>
        new List<string>()
        {
            "Liavine", "Murissine", "Tausmium"
        };

    public static IEnumerable<string> DraecarraItems() =>
        new List<string>()
        {
            "Murissine", "Sangunalt"
        };

    public static IEnumerable<string> EutaniaItems() =>
        new List<string>()
        {
            "Avanthium", "Murissine", "Phytemicium", "Tausmium"
        };

    public static IEnumerable<string> NecykeItems() =>
        new List<string>()
        {
            "Avanthium", "Caelyrium", "Liavine"
        };

    public static IEnumerable<string> LuthienItems() =>
        new List<string>()
        {
            "Vohphos"
        };

    public static IEnumerable<string> PsigenItems() =>
        new List<string>()
        {
            "Bulrium"
        };

    public static IEnumerable<string> TsumaItems() =>
        new List<string>()
        {
            "Avanthium", "Caelyrium", "Mythine",
        };
}