using UnityEngine;

public static class StatInfluenceSystem
{
    public static void ApplyFromStats(PlayerStatManager stats, ref StatusEffectsSnapshot s)
    {
        var cfg = StatInfluenceProvider.Instance != null ? StatInfluenceProvider.Instance.config : null;
        if (cfg == null) return;

        float hunger01 = Safe01(stats.Hunger, stats.HungerMax);
        float hyd01 = Safe01(stats.Hydration, stats.HydrationMax);

        // Hunger influences
        s.StaminaRegenModifier *= cfg.hungerToStaminaRegen.Evaluate(hunger01);
        s.HealthDegenerationPerSecond += cfg.hungerToHealthDps.Evaluate(hunger01);

        // Hydration influences
        s.StaminaRegenModifier *= cfg.hydrationToStaminaRegen.Evaluate(hyd01);
        s.StaminaDrainMultiplier *= cfg.hydrationToStaminaDrain.Evaluate(hyd01);
        s.HealthDegenerationPerSecond += cfg.hydrationToHealthDps.Evaluate(hyd01);

        // Temperature influences
        float delta = Mathf.Abs(stats.Temperature - cfg.normalTemp);
        s.StaminaRegenModifier *= cfg.tempDeltaToStaminaRegen.Evaluate(delta);
        s.StaminaDrainMultiplier *= cfg.tempDeltaToStaminaDrain.Evaluate(delta);

        if (stats.Temperature < cfg.noSprintBelow || stats.Temperature > cfg.noSprintAbove)
            s.CanSprint = false;

        // Weight influences
        float load = stats.CurrentWeight / Mathf.Max(0.0001f, stats.MaxCarryWeight);
        s.MoveSpeedMultiplier *= cfg.overloadToMoveSpeed.Evaluate(load);
        s.StaminaDrainMultiplier *= cfg.overloadToStaminaDrain.Evaluate(load);

        if (load > 1f) s.CanSprint = false;
    }

    private static float Safe01(float v, float max)
    {
        if (max <= 0.0001f) return 0f;
        return Mathf.Clamp01(v / max);
    }
}
