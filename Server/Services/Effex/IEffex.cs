using GameLib.Data.GameItems.Consumable;

namespace Server.Services.Effex;

public interface IEffex
{
    object GetEffectsByPlanet(string planetName);
    List<ConsumableEffect> GetEffectsBySector(string sectorName);
    Tuple<bool, string> AddEffect(string sectorName, ConsumableEffect effect);
    void UseEffect(string sectorName, ConsumableEffect effect);
}