using System.Text.Json.Serialization;

namespace GameLib.Data.GameItems.Consumable;

public class ConsumableEffect
{
    public string GameItemName { get; set; } = null!;
    public bool IsBuff { get; set; }
    public int Charges { get; set; }

    [JsonIgnore] public double Modifier { get; set; }
    [JsonIgnore] public string Type { get; set; } = null!;
}