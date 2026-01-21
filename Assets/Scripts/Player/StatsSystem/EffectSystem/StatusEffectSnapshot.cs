[System.Serializable]
public struct StatusEffectsSnapshot
{
    public float HealthDegenerationPerSecond;
    // Pain
    public bool HasPain;
    public float PainIntensity;
    public float PainSuppression;

    // Movement & stamina
    public float MoveSpeedMultiplier;
    public float StaminaDrainMultiplier;
    public float StaminaRegenModifier;
    public float StaminaCapMultiplier;
    public bool CanSprint;

    // Rates
    public float HealthRegenModifier;
    public float HungerRateModifier;
    public float HydrationRateModifier;
    public float TemperatureRateModifier;
    public float EnergyRateModifier;

    // Damage
    public float BleedDpsBonus;

    public static StatusEffectsSnapshot Default => new StatusEffectsSnapshot
    {
        HealthDegenerationPerSecond = 0f,

        MoveSpeedMultiplier = 1f,
        StaminaDrainMultiplier = 1f,
        StaminaRegenModifier = 1f,
        StaminaCapMultiplier = 1f,
        CanSprint = true,

        PainIntensity = 0f,
        PainSuppression = 0f,

        HealthRegenModifier = 1f,
        HungerRateModifier = 1f,
        HydrationRateModifier = 1f,
        TemperatureRateModifier = 1f,
        EnergyRateModifier = 1f,

        BleedDpsBonus = 0f
    };
}
