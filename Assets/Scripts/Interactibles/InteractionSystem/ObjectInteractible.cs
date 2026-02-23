using System;
using System.Linq;
using UnityEngine;
using static ItemData;


// This script represents an interactible object, it has requirements to interact and has has list of actions that will happen after intereaction
public class ObjectInteractible : MonoBehaviour, IInteractable, IHoldInteractable, IHoldFeedback
{
    [SerializeField] string id;
    [SerializeField] string label;
    [SerializeField] private ObjectInteractible dependentOn;
    [SerializeField] protected bool isActive = true;
    public bool IsActive => isActive;
    public string Id => id;
    [Serializable]
    public class RequiredItem
    {
        public ItemData data;
        public int amount = 1;
        public float durabilityCost = 1f;
    }

    [Header("Requirements")]
    [SerializeField] private RequiredItem[] requiredItems;

    [Header("Hold")]
    [SerializeField] private float holdDuration = 1.0f;

    [Header("Costs")]
    [Range(0f, 100f)]
    [SerializeField] private float hungerCost;
    [Range(0f, 100f)]
    [SerializeField] private float hydrationCost;
    [Range(0f, 100f)]
    [SerializeField] private float energyCost;
    [Range(0, 23)]
    [SerializeField] private int timeHourCost;
    [Range(0, 59)]
    [SerializeField] private int timeMinuteCost;

    [Header("Quest Event")]
    [SerializeField] private bool raiseQuestEventOnComplete = true;
    [SerializeField] private string questInteractIdOverride;

    [Header("On Complete Actions")]
    [SerializeField] private InteractAction[] onCompleteActions;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
    public void ApplyStateImmediate(bool active)
    {
        isActive = active;
        gameObject.SetActive(active);
    }

    protected bool IsUnlocked()
    {
        if (!isActive) return false;
        if (dependentOn == null) return true;
        return dependentOn.IsActive == false;
    }

    public float GetInteractDuration(PlayerInteractor interactor)
    {
        if (!IsUnlocked()) return 0f;
        return HasRequiredItems() ? holdDuration : 0f;
    }

    public bool Interact(PlayerInteractor interactor)
    {
        if (!IsUnlocked()) return false;
        if (!HasRequiredItems()) return false;

        WithdrawCost();
        RaiseQuestInteract();
        RunOnCompleteActions(interactor);
        return true;
    }

    public void OnHoldCanceled(PlayerInteractor interactor) { }
    public void OnHoldStart(PlayerInteractor interactor, float duration) { }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        prompt = "";

        if (!isActive) return false;

        if (!IsUnlocked())
        {
            prompt = label + " Locked.";
            return true;
        }

        var parts = requiredItems
            .Where(x => x.data != null)
            .Select(x => $"{x.data.itemName} x({x.amount})");

        if (!HasRequiredItems())
        {
            prompt = label + " Need: " + string.Join(", ", parts);
            return true;
        }

        prompt = label + " Hold to interact. Need: " + string.Join(", ", parts);
        return true;
    }


    private bool HasRequiredItems()
    {
        var invMgr = InventoryManager.Instance;
        if (invMgr == null || invMgr.playerInventory == null) return false;

        var inv = invMgr.playerInventory;

        if (requiredItems == null || requiredItems.Length == 0)
            return true;

        foreach (var req in requiredItems)
        {
            if (req.data == null) return false;
            int have = inv.GetTotalAmountById(req.data.id);
            if (have < req.amount) return false;
        }

        return true;
    }

    private void WithdrawCost()
    {
        var invManager = InventoryManager.Instance;
        var statManager = PlayerStatManager.Instance;
        var dateWeatherManager = DateWeatherManager.Instance;

        if (dateWeatherManager != null)
            dateWeatherManager.AddTime(timeHourCost, timeMinuteCost);

        if (statManager != null)
        {
            statManager.ChangeHunger(-hungerCost);
            statManager.ChangeHydration(-hydrationCost);
            statManager.ChangeEnergy(-energyCost);
        }

        if (requiredItems == null) return;

        foreach (var reqItem in requiredItems)
        {
            if (reqItem.data == null) continue;

            if (reqItem.data.HasTag(ItemTag.Tool))
            {
                var tool = invManager.playerInventory.GetInventoryItemById(reqItem.data.id);
                if (tool != null)
                    tool.Damage(reqItem.durabilityCost);
            }
            else
            {
                invManager.playerInventory.RemoveItem(reqItem.data, reqItem.amount);
            }
        }
    }

    private void RaiseQuestInteract()
    {
        if (!raiseQuestEventOnComplete) return;

        var interactId = string.IsNullOrWhiteSpace(questInteractIdOverride) ? id : questInteractIdOverride;
        if (!string.IsNullOrWhiteSpace(interactId))
            GameEvents.RaiseInteracted(interactId);
    }

    private void RunOnCompleteActions(PlayerInteractor interactor)
    {
        if (onCompleteActions == null || onCompleteActions.Length == 0)
            return;

        var ctx = new InteractContext(this, interactor);

        foreach (var action in onCompleteActions)
        {
            if (action == null) continue;
            action.Execute(ctx);
        }
    }
}