using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/GearData")]
public class GearData : ItemData, IStatProvider
{
    public float temperatureResist = 0;
    public float damageResist = 0;

    public enum GearSlot { Head, Chest, Legs, Boots }
    public GearSlot slot;

    public override IEnumerable<StatValue> GetStats()
    {
        if (temperatureResist != 0) yield return new StatValue { id = StatId.TemperatureResist, value = temperatureResist };
        if (damageResist != 0) yield return new StatValue { id = StatId.DamageResist, value = damageResist };
    }
}
