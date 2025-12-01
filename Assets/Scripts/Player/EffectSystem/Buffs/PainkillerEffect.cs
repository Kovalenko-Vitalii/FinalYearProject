using UnityEngine;

public class PainkillerEffect : StatusEffect
{
    private readonly float painSuppression;

    public PainkillerEffect(float duration, float painSuppression, BodyPart? target = null)
        : base(StatusEffectId.Painkiller, duration, target)
    {
        this.painSuppression = painSuppression;
    }

    public override void ApplyTo(ref StatusEffectsSnapshot s)
    {
        s.PainSuppressed = true;

        float k = (1f - painSuppression);
        s.ScreenBlur *= k;
        s.VignetteIntensity *= k;
        s.DoubleVision *= k;
        s.PulseIntensity *= k;
    }

}
