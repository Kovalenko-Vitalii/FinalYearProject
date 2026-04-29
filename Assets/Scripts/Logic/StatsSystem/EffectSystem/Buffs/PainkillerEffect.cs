using UnityEngine;

public class PainkillerEffect : StatusEffect
{
    public float Suppression { get; private set; }
    public bool AllowsSprintThroughLegInjury => Suppression >= 0.35f;

    public PainkillerEffect(float duration, float suppression, BodyPart? target = null)
        : base(StatusEffectId.Painkiller, duration, target)
    {
        Suppression = Mathf.Clamp01(suppression);
    }

    public override bool TryMerge(StatusEffect other)
    {
        if (other is not PainkillerEffect pk || pk.TargetPart != TargetPart)
            return false;

        Duration = Mathf.Max(Duration, pk.Duration);

        float a = Suppression;
        float b = pk.Suppression;
        Suppression = Mathf.Clamp01(1f - (1f - a) * (1f - b));

        return true;
    }

    public override void ApplyTo(ref StatusEffectsSnapshot s)
    {
        s.PainSuppression = Mathf.Max(s.PainSuppression, Suppression);
    }
}
