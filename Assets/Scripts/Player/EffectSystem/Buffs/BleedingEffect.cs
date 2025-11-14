using UnityEngine;

public class BleedingEffect : StatusEffect
{
    private readonly float damagePerSecond;

    public BleedingEffect(float duration, float damagePerSecond)
        : base(StatusEffectId.Bleeding, duration)
    {
        this.damagePerSecond = damagePerSecond;
    }

    public override void Tick(PlayerStatManager stats, float deltaTime)
    {
        base.Tick(stats, deltaTime);
        stats.ChangeHealth(-damagePerSecond * deltaTime);
    }
}

