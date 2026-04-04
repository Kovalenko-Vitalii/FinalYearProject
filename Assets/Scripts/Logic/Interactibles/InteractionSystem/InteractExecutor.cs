using System;
using System.Linq;
using UnityEngine;
using static ItemData;

public class InteractExecutor : MonoBehaviour, ISaveable
{
    [SerializeField] private string id;
    [SerializeField] private string label = "Interact";
    [SerializeField] private string holdPromptSuffix = " Hold to interact.";

    [Header("State")]
    [SerializeField] private bool isActive = true;

    [Header("Requirements")]
    [SerializeField] private RequiredItem[] requiredItems;

    [SerializeField] private InteractExecutor dependentOn;

    [Header("Costs")]
    [Range(0f, 100f)] [SerializeField] private float hungerCost;
    [Range(0f, 100f)] [SerializeField] private float hydrationCost;
    [Range(0f, 100f)] [SerializeField] private float energyCost;
    [Range(0, 23)] [SerializeField] private int timeHourCost;
    [Range(0, 59)] [SerializeField] private int timeMinuteCost;

    [Header("=On Complete Actions=")]
    [Header("Quest interact event")]
    [SerializeField] private string questInteractIdOverride;

    [Header("Zone enter event")]
    [SerializeField] private string zoneId;

    [Header("Lines when completed")]
    [SerializeField] private LineData[] completionLines;

    [Header("Other")]
    [SerializeField] private InteractAction[] onCompleteActions;
    public bool IsActive => isActive;
    public string Label => label;
    public string SaveId => id;

    private void Reset()
    {
#if UNITY_EDITOR
        SaveIdUtil.EnsureId(ref id, this);
#else
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");
#endif
    }

#if UNITY_EDITOR
    private void OnValidate() => SaveIdUtil.EnsureId(ref id, this);
#endif

    public void ApplyStateImmediate(bool active)
    {
        isActive = active;
        gameObject.SetActive(active);
    }

    public bool IsUnlocked(ExecutePolicy policy)
    {
        if (!isActive) return false;
        if (policy.HasFlag(ExecutePolicy.IgnoreLock)) return true;
        if (dependentOn == null) return true;
        return dependentOn.IsActive == false;
    }

    public bool CanExecute(ExecutePolicy policy)
    {
        if (!IsUnlocked(policy)) return false;
        if (policy.HasFlag(ExecutePolicy.IgnoreRequirements)) return true;
        return HasRequiredItems();
    }

    public bool Execute(InteractContext ctx, ExecutePolicy policy = ExecutePolicy.Default)
    {
        if (!CanExecute(policy)) return false;

        if (!policy.HasFlag(ExecutePolicy.IgnoreCosts))
            WithdrawCost();

        RaiseQuestInteract();

        RaiseZoneEnteredBuiltIn();

        PlayBuiltInLines();

        RunActions(ctx);
        return true;
    }

    public bool TryGetPrompt(out string prompt, ExecutePolicy policy = ExecutePolicy.Default, bool includeHoldSuffix = false)
    {
        prompt = "";

        if (!isActive) return false;

        if (!IsUnlocked(policy))
        {
            prompt = $"{label} Locked.";
            return true;
        }

        if (policy.HasFlag(ExecutePolicy.IgnoreRequirements) || HasRequiredItems())
        {
            prompt = label + (includeHoldSuffix ? holdPromptSuffix : "");
            return true;
        }

        var parts = requiredItems
            .Where(x => x?.data != null)
            .Select(x => $"{x.data.itemName} x({x.amount})");

        prompt = label + " Need: " + string.Join(", ", parts);
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
        var dateWeatherManager = TimeEnviromentManager.Instance;

        if (dateWeatherManager != null)
            dateWeatherManager.AddTime(-timeHourCost, -timeMinuteCost);

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
                tool?.Damage(reqItem.durabilityCost);
            }
            else
            {
                invManager.playerInventory.RemoveItem(reqItem.data, reqItem.amount);
            }
        }
    }

    private void RaiseQuestInteract()
    {
        var interactId = string.IsNullOrWhiteSpace(questInteractIdOverride) ? id : questInteractIdOverride;
        if (!string.IsNullOrWhiteSpace(interactId))
            GameEvents.RaiseInteracted(interactId);
    }

    private void RaiseZoneEnteredBuiltIn()
    {
        if (string.IsNullOrWhiteSpace(zoneId)) return;
        GameEvents.RaiseZoneEntered(zoneId);
    }

    private void PlayBuiltInLines()
    {
        if (completionLines == null || completionLines.Length == 0) return;

        var subtitleService = SubtitleService.Instance;
        if (subtitleService == null) return;

        foreach (var line in completionLines)
        {
            if (line == null) continue;
            subtitleService.Play(line);
        }
    }

    private void RunActions(InteractContext ctx)
    {
        if (onCompleteActions == null || onCompleteActions.Length == 0) return;

        foreach (var action in onCompleteActions)
            action?.Execute(ctx);
    }

    // ---------------- ISaveable ----------------
    public object CaptureState()
    {
        return new InteractExecutorState
        {
            isActive = isActive
        };
    }

    public void RestoreState(object state)
    {
        if (state is not InteractExecutorState s) return;
        ApplyStateImmediate(s.isActive);
    }
}

[Serializable]
public class RequiredItem
{
    public ItemData data;
    public int amount = 1;
    public float durabilityCost = 1f;
}

[Serializable]
public struct InteractExecutorState
{
    public bool isActive;
}