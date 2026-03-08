using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BodyPart
{
    Head,
    Torso,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg
}

// This class responsible for managing player`s buffs/effects
// It holds list of various effects and ticks them
public class StatusEffectManager : MonoBehaviour, ISaveable
{  
    public static StatusEffectManager Instance { get; private set; }

    public string SaveId => "STATUS_EFFECT_MANAGER";

    // List of effects
    private readonly List<StatusEffect> effects = new();

    // Actions for control
    public event Action<StatusEffect> OnEffectAdded;
    public event Action<StatusEffect> OnEffectRemoved;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // This method ticks each effect
    public void TickEffects(float dt)
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var e = effects[i];
            e.Tick(dt);

            if (e.IsFinished)
            {
                e.OnExpire(PlayerStatManager.Instance);
                effects.RemoveAt(i);
                OnEffectRemoved?.Invoke(e);
            }
        }
    }

    public void ApplyAllTo(ref StatusEffectsSnapshot s)
    {
        foreach (var e in effects)
            e.ApplyTo(ref s);
    }

    // Adding effect to list
    public void AddEffect(StatusEffect effect, bool replaceSameOnSamePart = true)
    {
        // Checking if we have everything needed
        if (effect == null) return;

        var stats = PlayerStatManager.Instance;
        if (stats == null) return;

        // Checking if we can apply effect
        if (!StatusEffectRules.CanApplyTo(effect.Id, effect.TargetPart))
            return;

        // Iterating through each effect
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var existing = effects[i];

            // Check if id and bodypart match
            if (existing.Id == effect.Id && existing.TargetPart == effect.TargetPart)
            {
                // Try to merge
                if (existing.TryMerge(effect))
                {
                    return;
                }

                // Deleting if asked and stopping for cycle
                if (replaceSameOnSamePart)
                {
                    /*
                    existing.OnExpire(stats);
                    effects.RemoveAt(i);
                    OnEffectRemoved?.Invoke(existing);
                    */

                    RemoveEffect(effect.Id, effect.TargetPart, true);
                    break;
                }
            }
        }

        // If this code reached this means no suitable effect for merge was not found or replace is requested
        // Simply adding new effect
        effects.Add(effect);
        effect.OnApply(stats);
        OnEffectAdded?.Invoke(effect);
    }


    // This method removes effect
    public void RemoveEffect(StatusEffectId id, BodyPart? part = null, bool deleteOne = false)
    {
        var stats = PlayerStatManager.Instance;
        if (stats == null) return;

        // Iterating
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var e = effects[i];
            // If found with same Id and bodypart
            if (e.Id == id && (part == null || e.TargetPart == part))
            {
                // Deleting
                e.OnExpire(stats);
                effects.RemoveAt(i);
                OnEffectRemoved?.Invoke(e);

                // Breaking cycke if asked
                if (deleteOne) break;
            }
        }

    }

    // True if there is effect with specified id
    public bool HasEffect(StatusEffectId id) => effects.Any(e => e.Id == id);

    // Getting all effects for specified body part
    public IReadOnlyList<StatusEffect> GetEffectsForPart(BodyPart part)
        => effects.Where(e => e.TargetPart == part).ToList();

    // Getting all effects that are not specified with body part
    public IReadOnlyList<StatusEffect> GetGlobalEffects()
        => effects.Where(e => !e.TargetPart.HasValue).ToList();

    // Getting list of bodyparts with any effects
    public IEnumerable<BodyPart> GetBodyPartsWithEffects()
        => effects.Where(e => e.TargetPart.HasValue)
                  .Select(e => e.TargetPart.Value)
                  .Distinct();

    // Removing all effects
    private void ClearAllInternal()
    {
        var stats = PlayerStatManager.Instance;
        if (stats != null)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].OnExpire(stats);
                OnEffectRemoved?.Invoke(effects[i]);
            }
        }

        effects.Clear();
    }

    // Capturing all effects for save
    public object CaptureState()
    {
        var data = new SaveEffectsData();

        foreach (var e in effects)
            data.effectList.Add(CaptureEffect(e));

        return data;
    }

    // Restoring all effects for save
    public void RestoreState(object state)
    {
        var data = state as SaveEffectsData;

        ClearAllInternal();

        if (data?.effectList == null) return;

        foreach (var s in data.effectList)
        {
            var effect = RestoreEffect(s);
            if (effect == null) continue;

            AddEffect(effect);
        }
    }

    // Capturing effect
    // I know it is not best approach
    public EffectSave CaptureEffect(StatusEffect e)
    {
        var baseSave = new EffectSave
        {
            id = e.Id,
            duration = e.Duration,
            hasTarget = e.TargetPart.HasValue,
            target = e.TargetPart.GetValueOrDefault(),
        };

        switch (e.Id)
        {
            case StatusEffectId.Bleeding:
                var b = (BleedingEffect)e;
                baseSave.payloadJson = JsonUtility.ToJson(new BleedingSave { dps = b.damagePerSecond });
                break;

            case StatusEffectId.Fracture:
                var f = (FractureEffect)e;
                baseSave.payloadJson = JsonUtility.ToJson(new FractureSave { speedMultiplier = f.speedMultiplier });
                break;

            case StatusEffectId.Pain:
                var p = (PainEffect)e;
                baseSave.payloadJson = JsonUtility.ToJson(new PainSave { intensity = p.Intensity, buildup = p.Intensity });
                break;
        }

        return baseSave;
    }

    // Restoring effect for save
    public StatusEffect RestoreEffect(EffectSave e)
    {
        BodyPart? target = e.hasTarget ? e.target : (BodyPart?)null;

        switch (e.id)
        {
            case StatusEffectId.Bleeding:
                var b = JsonUtility.FromJson<BleedingSave>(e.payloadJson);
                return new BleedingEffect(e.duration, b.dps, target);

            case StatusEffectId.Fracture:
                var f = JsonUtility.FromJson<FractureSave>(e.payloadJson);
                return new FractureEffect(e.duration, f.speedMultiplier, target ?? BodyPart.LeftLeg);

            case StatusEffectId.Pain:
                var p = JsonUtility.FromJson<PainSave>(e.payloadJson);
                return new PainEffect(e.duration, p.intensity, target);

            default: return null;
        }
    }
}

// Saving player`s effects
[Serializable]
public class SaveEffectsData
{
    public List<EffectSave> effectList = new();
}

// --- This system helps me to serialize different effects that has custom parameters, the best I could invent in this situation
[Serializable]
public class EffectSave
{
    public StatusEffectId id;
    public float duration;

    public bool hasTarget;
    public BodyPart target;

    public string payloadJson;
}

[Serializable]
public class BleedingSave
{
    public float dps;
}

[Serializable]
public class FractureSave
{
    public float speedMultiplier;
}

[Serializable]
public class PainSave
{
    public float intensity;
    public float buildup;
}