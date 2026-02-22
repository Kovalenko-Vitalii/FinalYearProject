using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemData;

// This script is responsible for managing quests
public class QuestManager : MonoBehaviour
{
    string TAG = "QuestManager"; 
    public static QuestManager Instance { get; private set; }

    private readonly Dictionary<string, QuestData> defs = new(); // Dictionary with all quests and their id`s
    private readonly Dictionary<string, QuestState> states = new(); // Progress across all quests

    private QuestData active; // Active quest
    public QuestData ActiveQuest => active;

    public event System.Action<QuestData> ActiveQuestChanged;
    public event System.Action<QuestData> QuestProgressChanged;

    private void Awake()
    {
        if (Instance != null)
        { 
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildDatabase();
        Subscribe();
    }

    private void OnDestroy()
    {
        if (Instance == this) Unsubscribe();
    }

    private void Subscribe()
    {
        GameEvents.Slept += OnSlept;
        GameEvents.Consumed += OnConsumed;
        GameEvents.ItemPicked += OnItemPicked;
        GameEvents.ZoneEntered += OnZoneEntered;
        GameEvents.Interacted += OnInteracted;
    }

    private void Unsubscribe()
    {
        GameEvents.Slept -= OnSlept;
        GameEvents.Consumed -= OnConsumed;
        GameEvents.ItemPicked -= OnItemPicked;
        GameEvents.ZoneEntered -= OnZoneEntered;
        GameEvents.Interacted -= OnInteracted;
    }

    // public api

    public void StartQuest(string questId)
    {
        if (!defs.TryGetValue(questId, out var def))
        {
            GameLog.Warning(TAG, $"Quest not found: {questId}");
            return;
        }

        SetActiveQuest(def);
        EnsureState(def);
        GameLog.Log(TAG, $"Active quest: {active.title}");
    }

    // Checking if quest completed
    public bool IsQuestCompleted(string questId)
        => states.TryGetValue(questId, out var state) && state.completed;

    public int GetStepProgress(string questId, int stepIndex)
    {
        if (!states.TryGetValue(questId, out var state)) return 0; // Checking if there even is state for quest
        if (stepIndex < 0 || stepIndex >= state.stepProgress.Count) return 0; // Checking if index is valid
        return state.stepProgress[stepIndex]; // Returning requested step progress
    }

    // === Handling events ===

    private void OnSlept() => TryProgress(StepType.Sleep, targetId: null, amount: 1);
    private void OnConsumed(string itemId, ItemTag tag, int amount)
    => TryProgress(StepType.Consume, itemId, amount, tag);

    private void OnItemPicked(string itemId, int amount)
        => TryProgress(StepType.PickupItem, itemId, amount);

    private void OnZoneEntered(string zoneId)
        => TryProgress(StepType.EnterZone, zoneId, 1);

    private void OnInteracted(string id)
        => TryProgress(StepType.Interact, id, 1);

    // === Core ===

    // This method is reading all events and trying to add a progress to active step
    // and complete quest if every step is satisfied
    private void TryProgress(StepType type, string targetId, int amount, ItemTag eventTag = ItemTag.None)
    {
        if (active == null) return; // Returning if there is no active quest

        var state = EnsureState(active); // Ensuring state
        if (state.completed) return; // If passed quest is completed returning

        bool changed = false; // Flag for event onchanged

        // Iterating through each step in active quest
        for (int i = 0; i < active.steps.Count; i++)
        {
            var step = active.steps[i]; // Getting steps
            if (step.type != type) continue; // Comparing type
            
            // If targedId exists checking if it same, if id not required doing same with tag
            if (!string.IsNullOrWhiteSpace(step.targetId))
            {
                if (step.targetId != targetId)
                    continue;
            }
            else if (step.requiredTag != ItemTag.None) // If tag is specified
            {
                if ((eventTag & step.requiredTag) == 0) // If we have at least one tag matching
                    continue;
            }

            int current = state.stepProgress[i]; // Taking current step progress to a buffer
            int next = Mathf.Min(current + amount, step.amount); // Calculating a new progress we will replace old one

            if (next != current) // If there is no progress we return
            {
                state.stepProgress[i] = next; // Applying new progress and setting changed flag as true
                changed = true;
            }
        }

        if (!changed) return; // If not changed we return
        QuestProgressChanged?.Invoke(active);

        // Checking if all steps are complete
        if (IsAllStepsComplete(active, state))
        {
            state.completed = true; // Setting state as complete
            GameLog.Log(TAG, $"Quest completed: {active.title}");

            PlayQuestCompleteLines(active); // Playing lines

            if (active.nextQuest != null) // Activating new quest if specified
            {
                SetActiveQuest(active.nextQuest);
                EnsureState(active);
                GameLog.Log(TAG, $"Next quest: {active.title}");
            }
            else
            {
                active = null;
            }
        }
    }

    // This should be moved to other separated module I think
    private void PlayQuestCompleteLines(QuestData quest)
    {
        if (quest == null) return;
        if (quest.onCompleteLines == null || quest.onCompleteLines.Count == 0) return;

        var subs = SubtitleService.Instance;
        if (subs == null) return;

        subs.PlaySequence(quest.onCompleteLines);
    }

    // This function is checking if all steps complete
    private bool IsAllStepsComplete(QuestData def, QuestState state)
    {
        for (int i = 0; i < def.steps.Count; i++)
        {
            if (state.stepProgress[i] < def.steps[i].amount)
                return false;
        }
        return true;
    }

    // This function is checking if quest has a state and if not it creates new one and returns it
    private QuestState EnsureState(QuestData def)
    {
        // Checking if there is a quest state at specified id
        if (states.TryGetValue(def.questId, out var existing))
        {
            // For saves if steps changed we need to update state
            while (existing.stepProgress.Count < def.steps.Count)
                existing.stepProgress.Add(0);
            return existing;
        }

        // Creating new state if there is no current one
        var s = new QuestState
        {
            questId = def.questId,
            completed = false,
            stepProgress = new List<int>(new int[def.steps.Count])
        };
        states[def.questId] = s;
        return s;
    }

    // This functuion is setting active quest and alerting that it changed
    private void SetActiveQuest(QuestData questData)
    {
        active = questData;
        ActiveQuestChanged?.Invoke(active);

        if (active != null)
            QuestProgressChanged?.Invoke(active);
    }

    // This function builds defs list of quests from Resources folder
    private void BuildDatabase()
    {
        defs.Clear();

        var loaded = Resources.LoadAll<QuestData>("Quests");

        foreach (var quest in loaded)
        {
            // Checking for invalid id`s and nulls
            if (quest == null || string.IsNullOrWhiteSpace(quest.questId)) continue;

            // Checking for dublicates
            if (defs.ContainsKey(quest.questId))
            {
                GameLog.Error(TAG, $"Duplicate questId: {quest.questId} ({quest.name})");
                continue;
            }

            // Adding quest
            defs[quest.questId] = quest;
        }
    }
}
