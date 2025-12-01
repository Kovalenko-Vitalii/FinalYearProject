using UnityEngine;

public class PainEffect : StatusEffect
{
    private readonly float intensity;

    public PainEffect(float duration, float intensity, BodyPart? target = null)
        : base(StatusEffectId.Pain, duration, target)
    {
        this.intensity = intensity;
    }

    public override void ApplyTo(ref StatusEffectsSnapshot s)
    {
        s.HasPain = true;
        s.PainIntensity = Mathf.Max(s.PainIntensity, intensity);

        s.ScreenBlur = Mathf.Max(s.ScreenBlur, intensity * 0.6f);
        s.VignetteIntensity = Mathf.Max(s.VignetteIntensity, intensity * 0.8f);

        float strong = Mathf.Clamp01((intensity - 0.5f) * 2f);
        s.DoubleVision = Mathf.Max(s.DoubleVision, strong * 0.7f);
        s.PulseIntensity = Mathf.Max(s.PulseIntensity, intensity);
    }

}

