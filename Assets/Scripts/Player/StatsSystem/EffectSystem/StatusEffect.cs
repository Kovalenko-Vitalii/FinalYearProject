// This class represents Effect that player can have
public abstract class StatusEffect
{
    // Type of effect
    public StatusEffectId Id { get; }
    // Duration
    public float Duration { get; protected set; }
    // Check if it is finished
    public bool IsFinished => Duration <= 0f;
    // Body part it is on
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

