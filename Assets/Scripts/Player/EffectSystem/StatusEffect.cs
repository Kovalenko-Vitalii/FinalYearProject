public abstract class StatusEffect
{
    public StatusEffectId Id { get; }
    public float Duration { get; protected set; }
    public bool IsFinished => Duration <= 0f;
    public BodyPart? TargetPart { get; }

    protected StatusEffect(StatusEffectId id, float duration, BodyPart? targetPart = null)
    {
        Id = id;
        Duration = duration;
        TargetPart = targetPart;
    }

    public virtual void OnApply(PlayerStatManager stats) { }
    public virtual void OnExpire(PlayerStatManager stats) { }

    public virtual void Tick(PlayerStatManager stats, float deltaTime)
    {
        Duration -= deltaTime;
    }

    public virtual void ApplyTo(ref StatusEffectsSnapshot snapshot) { }
    public virtual bool TryMerge(StatusEffect other) => false;
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

