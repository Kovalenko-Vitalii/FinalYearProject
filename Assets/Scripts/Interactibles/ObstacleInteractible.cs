using System;
using System.Linq;
using UnityEngine;

public class ObstacleInteractible : MonoBehaviour, IInteractable, IHoldInteractable, IHoldFeedback
{
    [SerializeField] requiredItem[] requiredItems;
    [SerializeField] float duration;
    [SerializeField] float hungerCost;
    [SerializeField] float hydrationCost;
    [SerializeField] float energyCost;
    [SerializeField] int timeHourCost;
    [SerializeField] int timeMinuteCost;

    [Serializable]
    public class requiredItem
    {
        public ItemData data;
        public int amount = 1;
        public float durabilityCost = 1;
        
    }

    public float GetInteractDuration(PlayerInteractor interactor)
    {
        return duration;
    }

    public bool Interact(PlayerInteractor interactor)
    {
        var inv = InventoryManager.Instance.playerInventory;

        foreach (var req in requiredItems)
        {
            if (req.data == null) return false;

            if (!req.data.HasTag(ItemData.ItemTag.Tool))
            {
                int have = inv.GetTotalAmountById(req.data.id);
                if (have < req.amount)
                    return false;
            }
            else
            {
                int haveTools = inv.GetTotalAmountById(req.data.id);
                if (haveTools < req.amount)
                    return false;
            }
        }

        WithdrawCost();
        gameObject.SetActive(false);
        return true;
    }


    public void OnHoldCanceled(PlayerInteractor interactor) { }

    public void OnHoldStart(PlayerInteractor interactor, float duration) { }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        var parts = requiredItems
            .Where(x => x.data != null)
            .Select(x => $"{x.data.itemName} x({x.amount})");

        prompt = "To break this obstacle you need: " + string.Join(", ", parts);
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
