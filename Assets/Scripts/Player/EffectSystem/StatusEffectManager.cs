using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }

    private readonly List<StatusEffect> effects = new();

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

    private void Update()
    {
        var stats = PlayerStatManager.Instance;
        if (stats == null || effects.Count == 0)
            return;

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
    }

    public void AddEffect(StatusEffect effect, bool replaceSame = true)
    {
        if (effect == null) return;
        var stats = PlayerStatManager.Instance;
        if (stats == null) return;

        if (replaceSame)
            RemoveEffect(effect.Id);

        effects.Add(effect);
        effect.OnApply(stats);
        OnEffectAdded?.Invoke(effect);
    }

    public void RemoveEffect(StatusEffectId id)
    {
        var stats = PlayerStatManager.Instance;
        if (stats == null) return;

        for (int i = effects.Count - 1; i >= 0; i--)
        {
            if (effects[i].Id == id)
            {
                var e = effects[i];
                e.OnExpire(stats);
                effects.RemoveAt(i);
                OnEffectRemoved?.Invoke(e);
            }
        }
    }

    public bool HasEffect(StatusEffectId id) => effects.Any(e => e.Id == id);
}
