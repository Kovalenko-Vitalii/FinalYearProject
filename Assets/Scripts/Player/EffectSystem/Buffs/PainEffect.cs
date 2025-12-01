using UnityEngine;

public class PainEffect : StatusEffect
{
    public float Intensity { get; private set; }

    public PainEffect(float duration, float intensity, BodyPart? target = null)
        : base(StatusEffectId.Pain, duration, target)
    {
        Intensity = Mathf.Clamp01(intensity);
    }

    public override bool TryMerge(StatusEffect other)
    {
        if (other is not PainEffect p || p.TargetPart != TargetPart)
            return false;

        Intensity = Mathf.Clamp01(Intensity + p.Intensity);
        Duration = Mathf.Max(Duration, p.Duration);

        return true;
    }

    public override void ApplyTo(ref StatusEffectsSnapshot s)
    {
        s.HasPain = true;
        s.PainIntensity = Mathf.Max(s.PainIntensity, Intensity);
    }
}
