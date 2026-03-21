using UnityEngine;

public class InventoryScreen : MenuScreen
{
    [Header("Inventory Screen")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private GearUI gearUI;
    [SerializeField] private EquipmentSelectionUI gearSelectionUI;
    [SerializeField] private ItemInfoUI itemInfoUI;

    [SerializeField] private GearData.GearSlot defaultSlot = GearData.GearSlot.Head;

    public override void OnOpen()
    {
        base.OnOpen();

        if (inventoryUI != null)
        {
            inventoryUI.Refresh();
        }

        if (gearUI != null)
            gearUI.Refresh();

        if (itemInfoUI != null)
            itemInfoUI.ShowDefault();
    }

    public override void OnClose()
    {
        base.OnClose();
    }
}
