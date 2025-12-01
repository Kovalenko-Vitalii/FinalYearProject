using UnityEngine;

public class WorldContainer : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int slotLimit = -1;
    [SerializeField] private string[] allowedTags;

    [Header("Seed items (optional)")]
    [SerializeField] private ItemData[] startItems;
    [SerializeField] private int[] startAmounts;

    public Inventory Inventory { get; private set; }

    private void Awake()
    {
        Inventory = new Inventory(new StorageInventoryPolicy(slotLimit, allowedTags));
        if (startItems != null)
        {
            for (int i = 0; i < startItems.Length; i++)
            {
                int amt = (startAmounts != null && i < startAmounts.Length) ? startAmounts[i] : 1;
                Inventory.AddItem(startItems[i], amt, startItems[i].maxDurability);
            }
        }
    }
}

