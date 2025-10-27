using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/GearData")]
public class GearData : ItemData, IStatProvider
{
    public float temperatureResist = 0;
    public float damageResist = 0;

    public enum GearSlot { Head, Chest, Legs, Boots }
    public GearSlot slot;

    public IEnumerable<StatValue> GetStats()
    {
        if (temperatureResist != 0) yield return new StatValue { id = StatId.TemperatureResist, value = temperatureResist };
        if (damageResist != 0) yield return new StatValue { id = StatId.DamageResist, value = damageResist };
        if (weight != 0) yield return new StatValue { id = StatId.Weight, value = weight }; 
    }

    public IEnumerable<StatValue> GetBaselineForCompare()
    {
        return System.Array.Empty<StatValue>();
    }
}
