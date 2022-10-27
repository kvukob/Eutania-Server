namespace GameLib.Data.GameItems.Base;

public class Foil
{
    public static IEnumerable<string> Types() =>
        new List<string>()
        {
            "Standard","Flame", "Evil", "Frost",
        };
}