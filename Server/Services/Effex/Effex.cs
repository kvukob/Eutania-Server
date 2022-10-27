using System.Collections.Concurrent;
using GameLib.Data.GameItems.Consumable;
using Server.Services.Satellite;

namespace Server.Services.Effex;

public class Effex : IEffex
{
    private readonly ILogger<HubSatellite> _logger;

    private readonly ConcurrentDictionary<string, List<ConsumableEffect>> _effects = new();

    public Effex(ILogger<HubSatellite> logger)
    {
        _logger = logger;
    }

    public object GetEffectsByPlanet(string planetName)
    {
        var abbr = planetName.ToUpper().Substring(0, 2);
        var effects = _effects
            .Where(kvp => kvp.Key
                .Contains(abbr))
            .Select(kvp => new
            {
                Sector = kvp.Key,
                Effects = kvp.Value
            });
        return effects;
    }

    public List<ConsumableEffect> GetEffectsBySector(string sectorName)
    {
        _effects.TryGetValue(sectorName, out var effects);
        Console.WriteLine(effects);
        return effects ?? new List<ConsumableEffect>();
    }

    // Adds an effect to a sector
    // Defaults to 50 charges per effect
    public Tuple<bool, string> AddEffect(string sectorName, ConsumableEffect effect)
    {
        if (_effects.ContainsKey(sectorName))
        {
            var activeEffects = _effects[sectorName];
            if (activeEffects.Count < 3)
                activeEffects.Add(effect);
            else
                return new Tuple<bool, string>(false, "Sector already has three auras applied.");
        }
        else
        {
            _effects.TryAdd(sectorName, new List<ConsumableEffect>()
            {
                effect
            });
        }

        return new Tuple<bool, string>(true, "");
    }

    public void UseEffect(string sectorName, ConsumableEffect effect)
    {
        _effects.TryGetValue(sectorName, out var effects);
        if (effects is null) return;
        foreach (var cE in effects.Where(cE => cE == effect))
        {
            effect.Charges -= 1;
            if (effect.Charges <= 0)
            {
                effects.Remove(effect);
            }
        }
    }
}