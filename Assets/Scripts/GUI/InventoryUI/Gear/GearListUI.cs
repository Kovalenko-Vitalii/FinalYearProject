using System.Collections.Generic;
using UnityEngine;

public class GearUI : MonoBehaviour
{
    [SerializeField] private List<GearSlotUI> slotsUI = new List<GearSlotUI>();
    [SerializeField] private List<HeldSlotUI> heldUI = new List<HeldSlotUI>();

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnPlayerInventoryChanged += Refresh;
            InventoryManager.Instance.OnHeldEquipmentChanged += Refresh;
        }

        if (InventoryManager.Instance != null && InventoryManager.Instance.playerEquipment != null)
            InventoryManager.Instance.playerEquipment.OnChanged += OnGearChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnPlayerInventoryChanged -= Refresh;
            InventoryManager.Instance.OnHeldEquipmentChanged -= Refresh;
        }

        if (InventoryManager.Instance != null && InventoryManager.Instance.playerEquipment != null)
            InventoryManager.Instance.playerEquipment.OnChanged -= OnGearChanged;
    }

    private void OnGearChanged(GearData.GearSlot slot, GearData oldGear, GearData newGear)
    {
        Refresh();
    }

    public void Refresh()
    {
        var im = InventoryManager.Instance;
        if (im == null) return;

        var equipment = im.playerEquipment;
        var heldEquipment = im.playerHeldEquipment;

        if (equipment != null)
        {
            foreach (var slot in slotsUI)
            {
                var gearData = equipment.GetEquipped(slot.ToGearSlot());
                slot.SetItem(gearData);
            }
        }

        if (heldEquipment != null)
        {
            if (heldUI.Count > 0)
                heldUI[0].SetItem(heldEquipment.GetEquippedItem(HeldSlot.Slot1));

            if (heldUI.Count > 1)
                heldUI[1].SetItem(heldEquipment.GetEquippedItem(HeldSlot.Slot2));
        }
    }
}