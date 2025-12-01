using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int amount;

    public float currentDurability;

    public InventoryItem(ItemData data, int amount, float currentDurability = -1f)
    {
        this.data = data;
        this.amount = amount;

        if (currentDurability >= 0f)
            this.currentDurability = currentDurability;      
        else if (data != null && data.hasDurability)
            this.currentDurability = data.maxDurability; 
        else
            this.currentDurability = 0f;
    }

    public bool HasDurability => data != null && data.hasDurability;

    public bool Damage(float amountToDamage)
    {
        if (!HasDurability || amountToDamage <= 0f)
            return false;

        currentDurability -= amountToDamage; 

        Debug.Log($"[InventoryItem] {data.itemName} durability -> {currentDurability}");

        if (currentDurability <= 0f)
        {
            currentDurability = 0f;
            return true;
        }

        return false;
    }
}

