using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldItem : MonoBehaviour, IInteractable
{
    [Header("Data")]
    public ItemData data;
    public int amount = 1;

    public float currentDurability;

    public void Init(ItemData d, int amt, float currentDurability)
    {
        data = d;
        amount = Mathf.Max(amt, 0);
        this.currentDurability = currentDurability;
    }

    public void AddAmount(int delta)
    {
        amount += delta;
        if (amount <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }

    public int TryPickupTo(Inventory target, int desiredAmount)
    {
        if (target == null || data == null || amount <= 0 || desiredAmount <= 0)
            return 0;

        int toTake = Mathf.Min(desiredAmount, amount);
        int accepted = target.AddItemAndGetAccepted(data, toTake, currentDurability);
        if (accepted > 0) AddAmount(-accepted);
        return accepted;
    }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        if (amount <= 0 || data == null)
        {
            prompt = null;
            return false;
        }

        prompt = $"{data.itemName}";
        return true;
    }

    public bool Interact(PlayerInteractor interactor)
    {
        if (amount <= 0 || data == null) return false;

        int desired = amount;
        int taken = TryPickupTo(interactor.PlayerInventory, desired);
        return taken > 0;
    }
}
