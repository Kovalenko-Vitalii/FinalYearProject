using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemData;

// This class represents quest static information (text, name, steps etc.)
// For now very basic
[CreateAssetMenu(menuName = "Plot/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questId;
    public string title;
    [TextArea] public string description;

    [Header("Steps (all must be completed)")]
    public List<QuestStep> steps = new();

    public QuestData nextQuest;

    [Header("On Completed")]
    public List<LineData> onCompleteLines = new();
}

// This class represents dynamic quest parameters (progress etc.)
[Serializable]
public class QuestState
{
    public string questId;
    public bool completed;
    public List<int> stepProgress = new();
}

// This class represents step static data (type, id etc.)
[Serializable]
public class QuestStep
{
    public StepType type;

    public string description;

    [Header("ID IS ALWAYS IN PRIORITY")]
    public string targetId;
    public int amount = 1;
    public ItemTag requiredTag = ItemTag.None;
}
public enum StepType
{
    Sleep,
    Consume,
    PickupItem,
    EnterZone,
    Interact
}