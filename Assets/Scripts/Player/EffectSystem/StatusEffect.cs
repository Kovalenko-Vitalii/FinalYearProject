using UnityEngine;

public abstract class StatusEffect
{
    public StatusEffectId Id { get; }
    public float Duration { get; protected set; }
    public bool IsFinished => Duration <= 0f;

    protected StatusEffect(StatusEffectId id, float duration)
    {
        Id = id;
        Duration = duration;
    }

    public virtual void OnApply(PlayerStatManager stats) { }
    public virtual void OnExpire(PlayerStatManager stats) { }

    public virtual void Tick(PlayerStatManager stats, float deltaTime)
    {
        Duration -= deltaTime;
    }
}


public enum StatusEffectId
{
    Bleeding,
    HeavyBleeding,
    Pain,
    Painkiller,
    Fracture,
    Fatigue,
}

