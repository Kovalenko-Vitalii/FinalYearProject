using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConsumableStatusOp
{
    public enum OpType
    {
        None,
        AddEffect,
        RemoveEffect
    }

    public OpType opType = OpType.None;
    public StatusEffectId effectId;

    [Header("Parameters for AddEffect")]
    public float duration = 10f;
    public float magnitude = 1f;

    [Header("Targeting")]
    public bool affectAllParts = true;
    public BodyPart targetPart;
}

[CreateAssetMenu(menuName = "Items/ConsumableData")]
public class ConsumableData : ItemData, IStatProvider
{
    public float hpRestore;
    public float hungerRestore;
    public float hydrationRestore;
    public float temperatureRestore;

    public AudioClip onConsumeSound;

    [Header("Status effect operations")]
    public List<ConsumableStatusOp> statusOps = new(); 

    public override IEnumerable<StatValue> GetStats()
    {
        if (weight != 0) yield return new StatValue { id = StatId.Weight, value = weight };
        if (hpRestore != 0) yield return new StatValue { id = StatId.HpRestore, value = hpRestore };
        if (hungerRestore != 0) yield return new StatValue { id = StatId.HungerRestore, value = hungerRestore };
        if (hydrationRestore != 0) yield return new StatValue { id = StatId.HydrationRestore, value = hydrationRestore };
        if (temperatureRestore != 0) yield return new StatValue { id = StatId.TemperatureRestore, value = temperatureRestore };
    }
}