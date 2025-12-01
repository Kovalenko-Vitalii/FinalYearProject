public struct StatusEffectsSnapshot
{
    public bool HasPain;
    public float PainIntensity;
    public bool PainSuppressed;

    public float MoveSpeedMultiplier;
    public float StaminaDrainMultiplier;

    public float ScreenBlur;
    public float VignetteIntensity;
    public float DoubleVision;
    public float PulseIntensity;

    public float HealthRegenModifier;
    public float BleedDpsBonus;

    public bool CanSprint;
    public static StatusEffectsSnapshot Default => new StatusEffectsSnapshot
    {
        MoveSpeedMultiplier = 1f,
        StaminaDrainMultiplier = 1f,
        CanSprint = true,
        PainIntensity = 0f,
        ScreenBlur = 0f,
        VignetteIntensity = 0f,
        DoubleVision = 0f,
        PulseIntensity = 0f
    };
}
