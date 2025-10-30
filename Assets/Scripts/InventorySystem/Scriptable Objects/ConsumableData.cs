using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/ConsumableData")]
public class ConsumableData : ItemData, IStatProvider
{
    public float hpRestore = 0;
    public float hungerRestore = 0;
    public float hydrationRestore = 0;
    public float temperatureRestore = 0;
    public float durability = 0;

    public IEnumerable<StatValue> GetStats()
    {
        if (hpRestore != 0) yield return new StatValue { id = StatId.HpRestore, value = hpRestore };
        if (hungerRestore != 0) yield return new StatValue { id = StatId.HungerRestore, value = hungerRestore };
        if (hydrationRestore != 0) yield return new StatValue { id = StatId.HydrationRestore, value = hydrationRestore };
        if (temperatureRestore != 0) yield return new StatValue { id = StatId.TemperatureRestore, value = temperatureRestore };
        if (durability != 0) yield return new StatValue { id = StatId.Durability, value = durability };
        if (weight != 0) yield return new StatValue { id = StatId.Weight, value = weight };
    }

    public IEnumerable<StatValue> GetBaselineForCompare()
    {
        return System.Array.Empty<StatValue>();
    }
}
