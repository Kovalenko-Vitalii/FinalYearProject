using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StatusEffectRules
{
    private static Dictionary<StatusEffectId, StatusEffectConfig> configs;

    public static void Init(StatusEffectConfig[] allConfigs)
    {
        configs = allConfigs.ToDictionary(c => c.id, c => c);
    }

    public static StatusEffectConfig GetConfig(StatusEffectId id)
    {
        if (configs != null && configs.TryGetValue(id, out var cfg))
            return cfg;
        return null;
    }

    public static bool CanApplyTo(StatusEffectId id, BodyPart? part)
    {
        if (!part.HasValue)
            return true;

        var cfg = GetConfig(id);
        if (cfg == null || cfg.allowedParts == null || cfg.allowedParts.Length == 0)
            return true;

        foreach (var p in cfg.allowedParts)
        {
            if (p == part.Value)
                return true;
        }

        return false;
    }
}
