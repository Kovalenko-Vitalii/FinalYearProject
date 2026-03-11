using UnityEngine;

public class FractureEffect : StatusEffect
{
    public float speedMultiplier;

    public FractureEffect(float duration, float speedMultiplier, BodyPart targetPart)
        : base(StatusEffectId.Fracture, duration, targetPart)
    {
        this.speedMultiplier = speedMultiplier;
    }

    public override void ApplyTo(ref StatusEffectsSnapshot s)
    {
        if (TargetPart == BodyPart.LeftLeg || TargetPart == BodyPart.RightLeg)
        {
            s.MoveSpeedMultiplier = Mathf.Min(s.MoveSpeedMultiplier, speedMultiplier);
            if (speedMultiplier <= 0.6f)
                s.CanSprint = false;
        }
    }
}

