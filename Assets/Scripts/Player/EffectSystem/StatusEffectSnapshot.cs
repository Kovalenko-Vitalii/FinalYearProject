public struct StatusEffectsSnapshot
{
    public bool HasPain;
    public float PainIntensity;
    public float PainSuppression;

    public float MoveSpeedMultiplier;
    public float StaminaDrainMultiplier;

    public float HealthRegenModifier;
    public float BleedDpsBonus;

    public bool CanSprint;

    public static StatusEffectsSnapshot Default => new StatusEffectsSnapshot
    {
        MoveSpeedMultiplier = 1f,
        StaminaDrainMultiplier = 1f,
        CanSprint = true,
        PainIntensity = 0f,
        PainSuppression = 0f,
        HealthRegenModifier = 1f,
        BleedDpsBonus = 0f
    };
}
