using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour, ISaveable
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
    [SerializeField] private ItemData[] initialItems;
    [SerializeField] private int[] initialAmounts;


    public event Action OnPlayerInventoryChanged;

    public event Action<InventoryItem, Inventory> OnSelectionChanged;

    public string SaveId => "PLAYER_INVENTORY";

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
        if (initialItems != null)
        {
            for (int i = 0; i < initialItems.Length; i++)
            {
                int amount = (initialAmounts != null && i < initialAmounts.Length) ? initialAmounts[i] : 1;
                if (initialItems[i] is GearData gearData)
                {
                    playerEquipment.Equip(gearData);
                    continue;
                }

                playerInventory.AddItem(initialItems[i], amount, initialItems[i].maxDurability);
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

    public object CaptureState()
    {
        var data = new SaveInventoryData();

        // inventory
        foreach (var it in playerInventory.items)
        {
            if (it == null || it.data == null) continue;
            if (it.amount <= 0) continue;

            data.inventoryItems.Add(new InventoryItemSave
            {
                itemId = it.data.id,
                amount = it.amount,
                durability = it.currentDurability
            });
        }

        // gear
        foreach (var kv in playerEquipment.slots)
        {
            var gear = kv.Value;
            if (gear == null) continue;

            data.gearSlots.Add(new GearPairSave
            {
                slot = kv.Key,
                gearId = gear.id,
                durability = 0f
            });
        }

        return data;
    }

    public void RestoreState(object state)
    {
        var data = state as SaveInventoryData;

        playerInventory.items.Clear();

        playerEquipment.Unequip(GearData.GearSlot.Head);
        playerEquipment.Unequip(GearData.GearSlot.Chest);
        playerEquipment.Unequip(GearData.GearSlot.Legs);
        playerEquipment.Unequip(GearData.GearSlot.Boots);

        if (data == null)
        {
            OnPlayerInventoryChanged?.Invoke();
            ClearSelection();
            return;
        }

        if (data.inventoryItems != null)
        {
            foreach (var s in data.inventoryItems)
            {
                var itemData = ItemResolver.Resolve(s.itemId);
                if (itemData == null)
                {
                    Debug.LogWarning($"[Inventory] Unknown itemId '{s.itemId}'");
                    continue;
                }

                playerInventory.AddItem(itemData, s.amount, s.durability);
            }
        }

        if (data.gearSlots != null)
        {
            foreach (var p in data.gearSlots)
            {
                if (string.IsNullOrWhiteSpace(p.gearId)) continue;

                var item = ItemResolver.Resolve(p.gearId);
                if (item == null)
                {
                    Debug.LogWarning($"[Inventory] Unknown gearId '{p.gearId}'");
                    continue;
                }

                if (item is not GearData gear)
                {
                    Debug.LogWarning($"[Inventory] itemId '{p.gearId}' is not GearData (got {item.GetType().Name})");
                    continue;
                }

                playerEquipment.Equip(gear);
            }
        }

        OnPlayerInventoryChanged?.Invoke();
        ClearSelection();
    }
}