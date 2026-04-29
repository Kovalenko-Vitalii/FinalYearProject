using UnityEngine;

public class InventoryScreen : MenuScreen
{
    [Header("Inventory Screen")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private EquipmentOverviewUI gearUI;
    [SerializeField] private EquipmentSelectionUI gearSelectionUI;
    [SerializeField] private ItemInfoUI itemInfoUI;

    [SerializeField] private EquipmentSlotId defaultSlot = EquipmentSlotId.Head;

    public override void OnOpen()
    {
        base.OnOpen();

        if (inventoryUI != null)
            inventoryUI.Refresh();

        if (gearUI != null)
            gearUI.Refresh();

        if (gearSelectionUI != null)
            gearSelectionUI.OpenSlot(defaultSlot);

        if (itemInfoUI != null)
            itemInfoUI.ShowDefault();
    }

    public override void OnClose()
    {
        base.OnClose();
    }
}
