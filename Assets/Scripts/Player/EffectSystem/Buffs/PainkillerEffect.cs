using UnityEngine;

public class PainkillerEffect : StatusEffect
{
    public float Suppression { get; private set; }

    public PainkillerEffect(float duration, float suppression, BodyPart? target = null)
        : base(StatusEffectId.Painkiller, duration, target)
    {
        Suppression = Mathf.Clamp01(suppression); // 0..1
    }

    public override bool TryMerge(StatusEffect other)
    {
        if (other is not PainkillerEffect pk || pk.TargetPart != TargetPart)
            return false;

        // по времени – берём максимальную длительность
        Duration = Mathf.Max(Duration, pk.Duration);

        // по силе – мультипликативное подавление: 1 - (1-a)*(1-b)
        float a = Suppression;
        float b = pk.Suppression;
        Suppression = Mathf.Clamp01(1f - (1f - a) * (1f - b));

        return true;
    }

    public override void ApplyTo(ref StatusEffectsSnapshot s)
    {
        // суммарная сила пейнкиллеров (по всем эффектам)
        s.PainSuppression = Mathf.Max(s.PainSuppression, Suppression);
        // никаких ScreenBlur / Vignette / Pulse тут тоже нет
    }
}
