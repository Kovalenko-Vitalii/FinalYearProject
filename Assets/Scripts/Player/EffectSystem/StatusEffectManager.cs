using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameplayOrchestrator;

public class StatusEffectManager : MonoBehaviour
{  
    public static StatusEffectManager Instance { get; private set; }

    private readonly List<StatusEffect> effects = new();
    public StatusEffectsSnapshot CurrentSnapshot { get; private set; }

    public event Action<StatusEffect> OnEffectAdded;
    public event Action<StatusEffect> OnEffectRemoved;
    public event Action<StatusEffectsSnapshot> OnSnapshotUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (GameplayOrchestrator.Instance.State != GameState.Gameplay) return;
        if (PauseManager.Instance.IsPaused) return;

        var stats = PlayerStatManager.Instance;
        if (stats == null || effects.Count == 0)
        {
            CurrentSnapshot = StatusEffectsSnapshot.Default;
            OnSnapshotUpdated?.Invoke(CurrentSnapshot);
            return;
        }

        float dt = Time.deltaTime;

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

            if (existing.Id == effect.Id &&
                existing.TargetPart == effect.TargetPart)
            {
                if (existing.TryMerge(effect))
                {
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
    }


    public void RemoveEffect(StatusEffectId id, BodyPart? part = null)
    {
        var stats = PlayerStatManager.Instance;
        if (stats == null) return;

        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var e = effects[i];
            if (e.Id == id && (part == null || e.TargetPart == part))
            {
                e.OnExpire(stats);
                effects.RemoveAt(i);
                OnEffectRemoved?.Invoke(e);
            }
        }
    }


    public bool HasEffect(StatusEffectId id) => effects.Any(e => e.Id == id);

    public IReadOnlyList<StatusEffect> GetEffectsForPart(BodyPart part)
    {
        return effects.Where(e => e.TargetPart == part).ToList();
    }

    public IReadOnlyList<StatusEffect> GetGlobalEffects()
    {
        return effects.Where(e => !e.TargetPart.HasValue).ToList();
    }

    public IEnumerable<BodyPart> GetBodyPartsWithEffects()
    {
        return effects
            .Where(e => e.TargetPart.HasValue)
            .Select(e => e.TargetPart.Value)
            .Distinct();
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

