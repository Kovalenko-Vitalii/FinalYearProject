using System.Collections.Generic;
using UnityEngine;

public class EquipmentOverviewUI : MonoBehaviour
{
    [SerializeField] private List<EquipmentSlotUI> slotsUI = new();

    private void OnEnable()
    {
        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnEquipmentChanged += Refresh;
            im.OnActiveHeldSlotChanged += HandleActiveHeldSlotChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnEquipmentChanged -= Refresh;
            im.OnActiveHeldSlotChanged -= HandleActiveHeldSlotChanged;
        }
    }

    private void HandleActiveHeldSlotChanged(EquipmentSlotId? _)
    {
        Refresh();
    }

    public void Refresh()
    {
        var im = InventoryManager.Instance;
        if (im == null)
            return;

        foreach (var slotUI in slotsUI)
        {
            if (slotUI == null)
                continue;

            var slot = slotUI.SlotId;
            var item = im.GetEquippedItem(slot);
            bool isActiveHeld = im.ActiveHeldSlot == slot;

            slotUI.SetItem(item, isActiveHeld);
        }
    }
}