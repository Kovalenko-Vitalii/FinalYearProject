using UnityEngine;

public static class StatInfluenceSystem
{
    public static void ApplyFromStats(PlayerStatManager stats, ref StatusEffectsSnapshot s)
    {
        var cfg = StatInfluenceProvider.Instance != null ? StatInfluenceProvider.Instance.config : null;
        if (cfg == null || stats == null) return;

        float hunger01 = Safe01(stats.Hunger, stats.HungerMax);
        float hyd01 = Safe01(stats.Hydration, stats.HydrationMax);

        // Hunger
        s.StaminaRegenModifier *= EvalMinMult01(hunger01, cfg.hungerToStaminaRegen);
        s.HealthDegenerationPerSecond += EvalDpsFromStart01(hunger01, cfg.hungerToStaminaRegen, cfg.hungerToHealthDpsAtZero);

        // Hydration
        s.StaminaRegenModifier *= EvalMinMult01(hyd01, cfg.hydrationToStaminaRegen);
        s.StaminaDrainMultiplier *= EvalMaxMult01(hyd01, cfg.hydrationToStaminaDrain);
        s.HealthDegenerationPerSecond += EvalDpsFromStart01(hyd01, cfg.hydrationToStaminaRegen, cfg.hydrationToHealthDpsAtZero);

        // Temperature
        float delta = Mathf.Abs(stats.Temperature - cfg.normalTemp);
        float tempT = Mathf.InverseLerp(cfg.tempPenaltyStartDelta, cfg.tempPenaltyFullDelta, delta); // 0..1

        if (tempT > 0f)
        {
            s.StaminaRegenModifier *= Mathf.Lerp(1f, cfg.tempMinStaminaRegenMult, tempT);
            s.StaminaDrainMultiplier *= Mathf.Lerp(1f, cfg.tempMaxStaminaDrainMult, tempT);
        }

        if (stats.Temperature < cfg.noSprintBelow || stats.Temperature > cfg.noSprintAbove)
            s.CanSprint = false;

        // Weight 
        float load = stats.CurrentWeight / Mathf.Max(0.0001f, stats.MaxCarryWeight);
        float loadT = Mathf.InverseLerp(cfg.overloadStart, cfg.overloadFull, load);

        if (loadT > 0f)
        {
            s.MoveSpeedMultiplier *= Mathf.Lerp(1f, cfg.overloadMinMoveSpeedMult, loadT);
            s.StaminaDrainMultiplier *= Mathf.Lerp(1f, cfg.overloadMaxStaminaDrainMult, loadT);
        }

        if (load > 1f) s.CanSprint = false;
    }

    private static float EvalMinMult01(float value01, MinMultRule r)
    {
        value01 = Mathf.Clamp01(value01);
        if (value01 >= r.start01) return 1f;

        float t = value01 / Mathf.Max(r.start01, 0.0001f);
        t = Mathf.Pow(t, Mathf.Max(0.1f, r.power));
        return Mathf.Lerp(r.minMult, 1f, t);
    }

    private static float EvalMaxMult01(float value01, MaxMultRule r)
    {
        value01 = Mathf.Clamp01(value01);
        if (value01 >= r.start01) return 1f;

        float t = value01 / Mathf.Max(r.start01, 0.0001f);
        t = Mathf.Pow(t, Mathf.Max(0.1f, r.power));
        return Mathf.Lerp(r.maxMult, 1f, t);
    }

    private static float Safe01(float v, float max)
    {
        if (max <= 0.0001f) return 0f;
        return Mathf.Clamp01(v / max);
    }

    private static float EvalDpsFromStart01(float value01, MinMultRule r, float dpsAtZero)
    {
        value01 = Mathf.Clamp01(value01);
        if (value01 >= r.start01) return 0f;

        float t = 1f - (value01 / Mathf.Max(r.start01, 0.0001f));
        t = Mathf.Clamp01(t);

        t = Mathf.Pow(t, Mathf.Max(0.1f, r.power));

        return dpsAtZero * t;
    }
}
