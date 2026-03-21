using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public string instanceId;
    public ItemData data;
    public int amount;
    public float currentDurability;

    public FirearmRuntimeState firearmState;

    public InventoryItem(ItemData data, int amount, float currentDurability = -1f)
    {
        instanceId = Guid.NewGuid().ToString();
        this.data = data;
        this.amount = amount;

        if (currentDurability >= 0f)
            this.currentDurability = currentDurability;      
        else if (data != null && data.hasDurability)
            this.currentDurability = data.maxDurability; 
        else
            this.currentDurability = 0f;

        EnsureRuntimeState();
    }

    public bool HasDurability => data != null && data.hasDurability;

    public bool IsFirearm => data is HoldableFirearmData;

    public void EnsureRuntimeState()
    {
        if (data is HoldableFirearmData)
        {
            firearmState ??= new FirearmRuntimeState();
        }
        else
        {
            firearmState = null;
        }
    }
    public void EnsureInstanceId()
    {
        if (string.IsNullOrWhiteSpace(instanceId))
            instanceId = Guid.NewGuid().ToString();
    }
    public InventoryItem Clone()
    {
        var copy = new InventoryItem(data, amount, currentDurability);

        if (firearmState != null)
        {
            copy.firearmState = new FirearmRuntimeState
            {
                currentAmmoInMag = firearmState.currentAmmoInMag
            };
        }

        return copy;
    }


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

[Serializable]
public class FirearmRuntimeState
{
    public int currentAmmoInMag;

    [NonSerialized] public bool isReloading;
    [NonSerialized] public float reloadProgress01;
}