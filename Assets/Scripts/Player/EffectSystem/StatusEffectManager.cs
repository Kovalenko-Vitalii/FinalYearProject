using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameplayOrchestrator;

public class StatusEffectManager : MonoBehaviour, IPlayerTick
{  
    public static StatusEffectManager Instance { get; private set; }

    private readonly List<StatusEffect> effects = new();
    public StatusEffectsSnapshot CurrentSnapshot { get; private set; } = StatusEffectsSnapshot.Default;

    public event Action<StatusEffect> OnEffectAdded;
    public event Action<StatusEffect> OnEffectRemoved;
    public event Action<StatusEffectsSnapshot> OnSnapshotUpdated;

    private void OnEnable()
    {
        if (PlayerTickSystem.Instance != null)
            PlayerTickSystem.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (PlayerTickSystem.Instance != null)
            PlayerTickSystem.Instance.Unregister(this);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Tick(float dt)
    {
        var stats = PlayerStatManager.Instance;
        if (stats == null)
        {
            SetSnapshot(StatusEffectsSnapshot.Default);
            return;
        }

        if (effects.Count == 0)
        {
            SetSnapshot(StatusEffectsSnapshot.Default);
            return;
        }

        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var e = effects[i];

            e.Tick(stats, dt);

            if (e.IsFinished)
            {
                e.OnExpire(stats);
                effects.RemoveAt(i);
                OnEffectRemoved?.Invoke(e);
            }
        }

        RebuildSnapshot();
    }

    private void SetSnapshot(StatusEffectsSnapshot snapshot)
    {
        if (Equals(CurrentSnapshot, snapshot)) return;

        CurrentSnapshot = snapshot;
        OnSnapshotUpdated?.Invoke(CurrentSnapshot);
    }

    private void RebuildSnapshot()
    {
        var snapshot = StatusEffectsSnapshot.Default;

        foreach (var e in effects)
        {
            e.ApplyTo(ref snapshot);
        }

        CurrentSnapshot = snapshot;
        OnSnapshotUpdated?.Invoke(CurrentSnapshot);
    }

    public void AddEffect(StatusEffect effect, bool replaceSameOnSamePart = true)
    {
        if (effect == null) return;

        var stats = PlayerStatManager.Instance;
        if (stats == null) return;

        if (!StatusEffectRules.CanApplyTo(effect.Id, effect.TargetPart))
            return;

        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var existing = effects[i];

            if (existing.Id == effect.Id && existing.TargetPart == effect.TargetPart)
            {
                if (existing.TryMerge(effect))
                {
                    RebuildSnapshot();
                    return;
                }

                if (replaceSameOnSamePart)
                {
                    existing.OnExpire(stats);
                    effects.RemoveAt(i);
                    OnEffectRemoved?.Invoke(existing);
                }
            }
        }

        effects.Add(effect);
        effect.OnApply(stats);
        OnEffectAdded?.Invoke(effect);

        RebuildSnapshot();
    }


    public void RemoveEffect(StatusEffectId id, BodyPart? part = null)
    {
        var stats = PlayerStatManager.Instance;
        if (stats == null) return;

        bool removedAny = false;

        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var e = effects[i];
            if (e.Id == id && (part == null || e.TargetPart == part))
            {
                e.OnExpire(stats);
                effects.RemoveAt(i);
                OnEffectRemoved?.Invoke(e);
                removedAny = true;
            }
        }

        if (removedAny)
            RebuildSnapshot();
    }


    public bool HasEffect(StatusEffectId id) => effects.Any(e => e.Id == id);

    public IReadOnlyList<StatusEffect> GetEffectsForPart(BodyPart part)
        => effects.Where(e => e.TargetPart == part).ToList();

    public IReadOnlyList<StatusEffect> GetGlobalEffects()
        => effects.Where(e => !e.TargetPart.HasValue).ToList();

    public IEnumerable<BodyPart> GetBodyPartsWithEffects()
        => effects.Where(e => e.TargetPart.HasValue)
                  .Select(e => e.TargetPart.Value)
                  .Distinct();
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
        RebuildSnapshot();
    }

    public SaveEffectsData CaptureAll()
    {
        var data = new SaveEffectsData();

        foreach (var e in effects)
            data.effectList.Add(CaptureEffect(e));

        return data;
    }

    public void RestoreAll(SaveEffectsData data)
    {
        if (data?.effectList == null) return;

        ClearAllInternal();

        foreach (var s in data.effectList)
        {
            var effect = RestoreEffect(s);
            AddEffect(effect);
        }
    }

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

public enum BodyPart
{
    Head,
    Torso,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg
}

