using System.Collections.Generic;
using UnityEngine;

// This class represents data for cloth
[CreateAssetMenu(menuName = "Items/GearData")]
public class GearData : ItemData, IStatProvider, IEquippableItemData
{
    public float temperatureResist = 0;
    public float damageResist = 0;

    public AudioClip onEquipSound;
    public AudioClip onUnequipSound;

    [SerializeField] private EquipmentSlotId slot = EquipmentSlotId.Head;
    public EquipmentSlotId Slot => slot;

    public IReadOnlyList<EquipmentSlotId> AllowedSlots => new[] { slot };

    public override IEnumerable<StatValue> GetStats()
    {
        if (weight != 0) yield return new StatValue { id = StatId.Weight, value = weight };
        if (temperatureResist != 0) yield return new StatValue { id = StatId.TemperatureResist, value = temperatureResist };
        if (damageResist != 0) yield return new StatValue { id = StatId.DamageResist, value = damageResist };
    }
}
