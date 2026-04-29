using UnityEngine;

// This class represents holdable meklee item 
[CreateAssetMenu(menuName = "Items/HoldableMeleeData")]
public class HoldableMeleeData : HoldableItemData
{
    [Header("== Melee Settings ==")]

    [Header("Generic")]
    public float hitDistance;
    public LayerMask hitMask = ~0;
    public int damageAmount = 0;
    public DamageType damageType = DamageType.Generic;
    public string swingTriggerName = "Swing";

    [Header("Sounds")]
    public AudioClip swingSound;
}
