using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public Inventory playerInventory;

    public InventoryItem SelectedItem { get; private set; }
    public Inventory SourceInventory { get; private set; }

    [Header("Settings")]
    [SerializeField] private int playerSlotLimit = 10;

    [Header("Player Gear")]
    public Equipment playerEquipment { get; private set; }

    [Header("Test items")]
    [SerializeField] private ItemData[] testItems;
    [SerializeField] private int[] testAmounts;


    public event Action OnPlayerInventoryChanged;

    public event Action<InventoryItem, Inventory> OnSelectionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInventory = new Inventory(new PlayerInventoryPolicy(playerSlotLimit));
        playerEquipment = new Equipment();

        playerInventory.OnChanged += () => OnPlayerInventoryChanged?.Invoke();
    }

    private void Start()
    {
        if (testItems != null)
        {
            for (int i = 0; i < testItems.Length; i++)
            {
                int amount = (testAmounts != null && i < testAmounts.Length) ? testAmounts[i] : 1;
                playerInventory.AddItem(testItems[i], amount, testItems[i].maxDurability);
            }
        }
    }

    public void MoveItem(Inventory from, Inventory to, ItemData data, int amount, float currentDurability)
    {
        if (from == null || to == null || data == null || amount <= 0) return;

        var src = from.items.Find(i => i.data == data);
        if (src == null || src.amount <= 0) return;

        int canMove = Mathf.Min(src.amount, amount);

        int beforeToCount = to.items.Where(i => i.data == data).Sum(i => i.amount);
        to.AddItem(data, canMove, currentDurability);
        int afterToCount = to.items.Where(i => i.data == data).Sum(i => i.amount);

        int accepted = Mathf.Clamp(afterToCount - beforeToCount, 0, canMove);
        if (accepted <= 0) return;

        from.RemoveItem(data, accepted);
    }

    public void SelectItem(InventoryItem item, Inventory source)
    {
        SelectedItem = item;
        SourceInventory = source;
        OnSelectionChanged?.Invoke(item, source);
    }

    public void ClearSelection()
    {
        SelectedItem = null;
        SourceInventory = null;
        OnSelectionChanged?.Invoke(null, null);
    }

    public SaveInventoryData Capture()
    {
        var gearList = new List<GearPair>();

        foreach (var kv in playerEquipment.slots)
        {
            gearList.Add(new GearPair
            {
                slot = kv.Key,
                gear = kv.Value
            });
        }

        return new SaveInventoryData
        {
            inventoryItems = playerInventory.items,
            gearSlots = gearList
        };
    }

    public void Restore(SaveInventoryData data)
    {
        playerInventory.items = data.inventoryItems;

        playerEquipment.Unequip(GearData.GearSlot.Head);
        playerEquipment.Unequip(GearData.GearSlot.Chest);
        playerEquipment.Unequip(GearData.GearSlot.Legs);
        playerEquipment.Unequip(GearData.GearSlot.Boots);

        if (data.gearSlots == null) return;

        foreach (var p in data.gearSlots)
        {
            if (p?.gear != null)
                playerEquipment.Equip(p.gear);
        }
    }


}






