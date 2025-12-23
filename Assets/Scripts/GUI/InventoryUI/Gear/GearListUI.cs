using System.Collections.Generic;
using UnityEngine;

public class GearUI : MonoBehaviour
{
    [SerializeField] private List<GearSlotUI> slotsUI = new List<GearSlotUI>();

    public void Refresh()
    {
        var equipment = InventoryManager.Instance.playerEquipment;

        foreach (var slot in slotsUI)
        {
            var gearData = equipment.GetEquipped(slot.ToGearSlot());

            if (gearData != null)
                slot.SetItem(gearData);
            else
                slot.SetItem(null);
        }
    }
}
