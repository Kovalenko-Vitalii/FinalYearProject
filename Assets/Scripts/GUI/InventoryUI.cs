using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private bool isPlayerInventory = true;
    [SerializeField] private bool isStorageInventory = false;

    private Inventory targetInventory => GetTargetInventory();

    private Inventory GetTargetInventory()
    {
        if (InventoryManager.Instance == null)
            return null;

        if (isPlayerInventory)
            return InventoryManager.Instance.playerInventory;

        if (isStorageInventory)
            return InventoryManager.Instance.storageInventory;

        return null;
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (targetInventory == null)
            return;

        foreach (Transform child in content)
            Destroy(child.gameObject);

        foreach (var item in targetInventory.items)
        {
            var obj = Instantiate(itemPrefab, content);
            obj.GetComponent<InventoryItemUI>().SetItem(item, targetInventory);
        }
    }
}
