using UnityEngine;

public static class StatusEffectFactory
{
    public static StatusEffect Create(StatusEffectId id, float duration, float magnitude, BodyPart? target = null)
    {
        switch (id)
        {
            case StatusEffectId.Painkiller:
                return new PainkillerEffect(duration, magnitude, target);

            case StatusEffectId.Pain:
                return new PainEffect(duration, magnitude, target);

            case StatusEffectId.Fracture:
                return new FractureEffect(duration, magnitude, target ?? BodyPart.LeftLeg);

            default:
                Debug.LogWarning($"No factory rule for StatusEffectId: {id}");
                return null;
        }
    }
}

