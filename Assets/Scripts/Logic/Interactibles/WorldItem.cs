using UnityEngine;

// This class represent inventory item instnace in game world
[RequireComponent(typeof(Collider))]
public class WorldItem : MonoBehaviour, IInteractable
{
    [Header("Runtime Item")]
    [SerializeField] private InventoryItem item;

    public InventoryItem Item => item;
    public ItemData Data => item?.data;
    public int Amount => item != null ? item.amount : 0;

    public void Init(InventoryItem inventoryItem)
    {
        item = inventoryItem;

        if (item != null)
            item.EnsureRuntimeState();
    }

    public void AddAmount(int delta)
    {
        if (item == null)
            return;

        item.amount += delta;

        if (item.amount <= 0)
            Destroy(gameObject);
    }

    public int TryPickupTo(Inventory target, int desiredAmount)
    {
        if (target == null || item == null || item.data == null || item.amount <= 0 || desiredAmount <= 0)
        {
            SoundManager.Instance?.PlayUI(UISoundId.RejectSound);
            return 0;
        }

        if (item.amount == 1)
        {
            bool accepted = target.TryAddItemInstance(item);
            if (!accepted)
            {
                SoundManager.Instance?.PlayUI(UISoundId.RejectSound);
                return 0;
            }

            SoundManager.Instance?.PlayUI(UISoundId.PickupSound, item.data.onPickupSound);
            GameEvents.RaiseItemPicked(item.data.id, 1);

            Destroy(gameObject);
            return 1;
        }

        int toTake = Mathf.Min(desiredAmount, item.amount);
        int acceptedAmount = target.AddItemAndGetAccepted(item.data, toTake, item.currentDurability);

        if (acceptedAmount > 0)
            AddAmount(-acceptedAmount);

        SoundManager.Instance?.PlayUI(UISoundId.PickupSound, item.data.onPickupSound);
        GameEvents.RaiseItemPicked(item.data.id, acceptedAmount);

        return acceptedAmount;
    }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        if (item == null || item.data == null || item.amount <= 0)
        {
            prompt = null;
            return false;
        }

        prompt = item.data.itemName;
        return true;
    }

    public bool Interact(PlayerInteractor interactor)
    {
        if (item == null || item.data == null || item.amount <= 0)
            return false;

        int taken = TryPickupTo(interactor.PlayerInventory, item.amount);
        return taken > 0;
    }

    public WorldItemSave Capture()
    {
        return new WorldItemSave
        {
            item = InventoryItemSave.FromRuntime(item),
            position = transform.position,
            rotation = transform.rotation
        };
    }

}
