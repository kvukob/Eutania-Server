using System.Collections.ObjectModel;

namespace GameLib.Data.Factions;

public static class GameFaction
{
    public const string Unassigned = "Unassigned";
    public const string KGelthua = "K'Gelthua";
    public const string Skyule = "Skyule";
    public const string Sargon = "S'argon";

    public static readonly ReadOnlyCollection<string> FactionList = new(new[]
    {
        Unassigned, KGelthua, Skyule, Sargon
    });

    public static bool IsValidFaction(string factionName)
    {
        return FactionList.Contains(factionName, StringComparer.OrdinalIgnoreCase);
    }
}