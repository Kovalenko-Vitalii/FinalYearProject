using UnityEngine;

public class ContainerUI : MonoBehaviour
{
    [Header("Player / Container UI")]
    [SerializeField] private InventoryUI playerInventoryUI;
    [SerializeField] private InventoryUI containerInventoryUI;

    public void ShowFor(WorldContainer worldContainer)
    {
        if (worldContainer == null) return;

        if (playerInventoryUI != null)
        {
            playerInventoryUI.SetTargetInventory(null);
            playerInventoryUI.Refresh();
        }

        if (containerInventoryUI != null)
        {
            containerInventoryUI.SetTargetInventory(worldContainer.Inventory);
            containerInventoryUI.Refresh();
        }
    }

    public void Clear()
    {
        if (containerInventoryUI != null)
            containerInventoryUI.SetTargetInventory(null);
    }
}
