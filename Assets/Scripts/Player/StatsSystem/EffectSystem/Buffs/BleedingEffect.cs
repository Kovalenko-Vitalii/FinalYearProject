public class BleedingEffect : StatusEffect
{
    public float damagePerSecond;

    public BleedingEffect(float duration, float damagePerSecond, BodyPart? targetPart = null)
        : base(StatusEffectId.Bleeding, duration, targetPart)
    {
        this.damagePerSecond = damagePerSecond;
    }

    public override void Tick(PlayerStatManager stats, float deltaTime)
    {
        base.Tick(stats, deltaTime);
        stats.ChangeHealth(-damagePerSecond * deltaTime);
    }
}
