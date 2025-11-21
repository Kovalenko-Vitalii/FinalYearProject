using System.Collections.Generic;

public static class StatusEffectRules
{
    private static readonly Dictionary<StatusEffectId, HashSet<BodyPart>> AllowedParts =
        new()
        {
            [StatusEffectId.Bleeding] = new HashSet<BodyPart>(
                (BodyPart[])System.Enum.GetValues(typeof(BodyPart))
            ),
       
            [StatusEffectId.Fracture] = new HashSet<BodyPart>
            {
                BodyPart.LeftLeg,
                BodyPart.RightLeg
            }
        };

    public static bool CanApplyTo(StatusEffectId id, BodyPart? part)
    {
        if (!part.HasValue)
            return true;

        if (!AllowedParts.TryGetValue(id, out var allowedSet))
            return true;

        return allowedSet.Contains(part.Value);
    }
}
