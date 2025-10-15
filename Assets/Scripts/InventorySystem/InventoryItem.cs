using UnityEngine;

[System.Serializable]
public class InventoryItem {
    public ItemData data;
    public int amount;

    public InventoryItem(ItemData data, int amount) {
        this.data = data;
        this.amount = amount;
    }
}

