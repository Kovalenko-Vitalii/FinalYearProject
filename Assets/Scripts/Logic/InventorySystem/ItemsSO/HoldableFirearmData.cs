using UnityEngine;

[CreateAssetMenu(menuName = "Items/HoldableFirearmData")]
public class HoldableFirearmData : HoldableItemData
{
    [Header("== Firearm Settings ==")]

    [Header("Firing")]
    public int damageAmount = 0;
    public float fireCooldown = 0f;
    public float fireDistance = 0f;
    public LayerMask hitMask = ~0;
    public DamageType damageType = DamageType.Bullet;

    [Header("Ammo")]
    public int ammoPerShot = 0;
    public int magCapacity = 0;
    public ItemData ammoItem;

    [Header("Recoil")]
    public float fireKickBack = 0f;
    public float fireKickUp = 0f;

    [Header("Sounds")]
    public AudioClip shotSound;
    public AudioClip dryShotSound;
    public AudioClip aimSound;
    public AudioClip reloadSound;
}
