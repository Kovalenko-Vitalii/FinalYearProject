using System;
using System.Linq;
using UnityEngine;

public class ObstacleInteractible : MonoBehaviour, IInteractable, IHoldInteractable, IHoldFeedback
{
    public string id;

    [SerializeField] requiredItem[] requiredItems;
    [SerializeField] float duration;
    [SerializeField] float hungerCost;
    [SerializeField] float hydrationCost;
    [SerializeField] float energyCost;
    [SerializeField] int timeHourCost;
    [SerializeField] int timeMinuteCost;

    [SerializeField] bool isActive = true;

    public string Id => id;
    public bool IsActive => isActive;

    public void ApplyStateImmediate(bool active)
    {
        isActive = active;
        gameObject.SetActive(isActive);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            GenerateId();
            return;
        }

        var all = FindObjectsByType<ObstacleInteractible>(FindObjectsSortMode.None);
        foreach (var c in all)
        {
            if (c == this) continue;
            if (c.id == id)
            {
                GenerateId();
                break;
            }
        }
    }

    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString("N");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    [Serializable]
    public class requiredItem
    {
        public ItemData data;
        public int amount = 1;
        public float durabilityCost = 1;
    }

    public float GetInteractDuration(PlayerInteractor interactor)
    {
        return HasRequiredItems() ? duration : 0f;
    }

    public bool Interact(PlayerInteractor interactor)
    {
        if (!isActive) return false;

        var inv = InventoryManager.Instance.playerInventory;

        foreach (var req in requiredItems)
        {
            if (req.data == null) return false;

            int have = inv.GetTotalAmountById(req.data.id);
            if (have < req.amount) return false;
        }

        WithdrawCost();
        ApplyStateImmediate(false);
        return true;
    }

    public void OnHoldCanceled(PlayerInteractor interactor) { }
    public void OnHoldStart(PlayerInteractor interactor, float duration) { }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        if (!isActive) 
        { 
            prompt = ""; 
            return false; 
        }

        var parts = requiredItems
            .Where(x => x.data != null)
            .Select(x => $"{x.data.itemName} x({x.amount})");

        if (!HasRequiredItems())
        {
            prompt = "Need: " + string.Join(", ", parts);
            return true;
        }

        prompt = "Hold to break. Need: " + string.Join(", ", parts);
        return true;
    }


    private bool HasRequiredItems()
    {
        if (!isActive) return false;

        var invMgr = InventoryManager.Instance;
        if (invMgr == null || invMgr.playerInventory == null) return false;

        var inv = invMgr.playerInventory;

        foreach (var req in requiredItems)
        {
            if (req.data == null) return false;

            int have = inv.GetTotalAmountById(req.data.id);
            if (have < req.amount) return false;
        }

        return true;
    }


    void WithdrawCost()
    {
        var invManager = InventoryManager.Instance;
        var statManager = PlayerStatManager.Instance;
        var dateWeatherManager = DateWeatherManager.Instance;

        dateWeatherManager.AddTime(timeHourCost, timeMinuteCost);
        statManager.ChangeHunger(-hungerCost);
        statManager.ChangeHydration(-hydrationCost);
        statManager.ChangeEnergy(-energyCost);

        foreach (var reqItem in requiredItems)
        {
            if (reqItem.data.HasTag(ItemData.ItemTag.Tool))
                invManager.playerInventory.GetInventoryItemById(reqItem.data.id).Damage(reqItem.durabilityCost);
            else
                invManager.playerInventory.RemoveItem(reqItem.data, reqItem.amount);
        }
    }
}
