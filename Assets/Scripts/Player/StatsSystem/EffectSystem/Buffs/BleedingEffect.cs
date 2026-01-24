public class BleedingEffect : StatusEffect
{
    public float damagePerSecond;

    public BleedingEffect(float duration, float damagePerSecond, BodyPart? targetPart = null)
        : base(StatusEffectId.Bleeding, duration, targetPart)
    {
        this.damagePerSecond = damagePerSecond;
    }

    public override void ApplyTo(ref StatusEffectsSnapshot s)
    {
        s.HealthDegenerationPerSecond += damagePerSecond;
    }

}
