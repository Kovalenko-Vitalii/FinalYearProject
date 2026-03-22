using System.Collections.Generic;
using UnityEngine;

public class HoldableItemData : ItemData, IEquippableItemData
{
    public GameObject firstPersonPrefab;

    public virtual IReadOnlyList<EquipmentSlotId> AllowedSlots => new[]
    {
        EquipmentSlotId.Held1,
        EquipmentSlotId.Held2
    };
}
