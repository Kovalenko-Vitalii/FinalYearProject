using UnityEngine;

[System.Serializable]
public struct MinMultRule
{
    [Range(0f, 1f)] public float start01;
    [Range(0.01f, 1f)] public float minMult;
    [Min(0.1f)] public float power;
}

[System.Serializable]
public struct MaxMultRule
{
    [Range(0f, 1f)] public float start01;
    [Min(1f)] public float maxMult;
    [Min(0.1f)] public float power;
}

[CreateAssetMenu(menuName = "Game/Stat Influence Config Simple")]
public class StatInfluenceConfig : ScriptableObject
{
    [Header("Hunger (0..1)")]
    public MinMultRule hungerToStaminaRegen = new() { start01 = 0.7f, minMult = 0.4f, power = 2f };
    public float hungerToHealthDpsAtZero = 0.2f;

    [Header("Hydration (0..1)")]
    public MinMultRule hydrationToStaminaRegen = new() { start01 = 0.7f, minMult = 0.3f, power = 2f };
    public MaxMultRule hydrationToStaminaDrain = new() { start01 = 0.7f, maxMult = 1.5f, power = 2f };
    public float hydrationToHealthDpsAtZero = 0.4f;

    [Header("Temperature (0..1)")]
    public MinMultRule temperatureToStaminaRegen = new() { start01 = 0.5f, minMult = 0.4f, power = 2f };
    public MaxMultRule temperatureToStaminaDrain = new() { start01 = 0.5f, maxMult = 1.5f, power = 2f };
    public float temperatureToHealthDpsAtZero = 0.5f;

    [Range(0f, 1f)] public float noSprintBelowTemperature01 = 0.2f;
    [Range(0f, 1f)] public float lowTemperatureMoveSpeedStart01 = 0.35f;
    [Range(0.01f, 1f)] public float lowTemperatureMinMoveSpeedMult = 0.85f;

    [Header("Weight (load = weight/maxCarry)")]
    public float overloadStart = 1f;
    public float overloadFull = 1.5f;
    [Range(0.01f, 1f)] public float overloadMinMoveSpeedMult = 0.6f;
    [Min(1f)] public float overloadMaxStaminaDrainMult = 2f;
}
