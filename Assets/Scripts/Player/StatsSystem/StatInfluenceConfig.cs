using UnityEngine;

[CreateAssetMenu(menuName = "Game/Stat Influence Config")]
public class StatInfluenceConfig : ScriptableObject
{
    [Header("Hunger (0..1)")]
    public AnimationCurve hungerToStaminaRegen = AnimationCurve.Linear(0, 0.4f, 1, 1f);
    public AnimationCurve hungerToStaminaCap = AnimationCurve.Linear(0, 0.7f, 1, 1f);
    public AnimationCurve hungerToHealthDps = AnimationCurve.Linear(0f, 0.2f, 1f, 0f);

    [Header("Hydration (0..1)")]
    public AnimationCurve hydrationToStaminaRegen = AnimationCurve.Linear(0, 0.3f, 1, 1f);
    public AnimationCurve hydrationToStaminaDrain = AnimationCurve.Linear(0, 1.5f, 1, 1f);
    public AnimationCurve hydrationToHealthDps = AnimationCurve.Linear(0f, 0.4f, 1f, 0f);

    [Header("Temperature")]
    public float normalTemp = 36.6f;
    public float noSprintBelow = 34f;
    public float noSprintAbove = 39f;
    public AnimationCurve tempDeltaToStaminaRegen = AnimationCurve.Linear(0, 1f, 10f, 0.5f);
    public AnimationCurve tempDeltaToStaminaDrain = AnimationCurve.Linear(0, 1f, 10f, 1.6f);

    [Header("Weight (load = weight/maxCarry)")]
    public AnimationCurve overloadToMoveSpeed = AnimationCurve.Linear(1f, 1f, 1.5f, 0.6f);
    public AnimationCurve overloadToStaminaDrain = AnimationCurve.Linear(1f, 1f, 1.5f, 2f);
}
