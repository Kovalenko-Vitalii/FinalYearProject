using UnityEngine;

[CreateAssetMenu(menuName = "Status Effects/Status Effect Config")]
public class StatusEffectConfig : ScriptableObject
{
    public StatusEffectId id;

    [Header("UI")]
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Rules")]
    public bool isPositive;
    public BodyPart[] allowedParts;
}
